using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    // 2次元グリッドによるワールド領域管理サービス
    public class WorldGridService
    {
        private readonly string _dbPath;
        private WorldLocation[,]? _grid;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public WorldGridService()
        {
            var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appData);
            _dbPath = Path.Combine(appData, "worldgrid.db");
        }

        // Load map files from Data/Maps/*.toml (simple parser for our map TOML structure)
        public void LoadMapsFromFiles()
        {
            var mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Maps");
            if (!Directory.Exists(mapDir)) return;
            var files = Directory.GetFiles(mapDir, "*.toml");
            if (files.Length == 0) return;

            // pick the first TOML file (simple approach)
            var text = File.ReadAllText(files[0]);
            try
            {
                // parse width/height
                int w = 0, h = 0;
                foreach (var line in text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var t = line.Trim();
                    if (t.StartsWith("width"))
                    {
                        var parts = t.Split('='); if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out var pw)) w = pw;
                    }
                    if (t.StartsWith("height"))
                    {
                        var parts = t.Split('='); if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out var ph)) h = ph;
                    }
                }
                if (w <= 0 || h <= 0) return;
                Width = w; Height = h;
                _grid = new WorldLocation[Width, Height];
                // initialize defaults
                for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    _grid[x, y] = new WorldLocation { X = x, Y = y, Name = $"Loc_{x}_{y}", Type = LocationType.Field, IsAccessible = true };
                }

                // parse location blocks
                var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
                int idx = 0;
                while (idx < lines.Length)
                {
                    var cur = lines[idx].Trim();
                    if (cur.StartsWith("[[locations]]"))
                    {
                        idx++;
                        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        while (idx < lines.Length)
                        {
                            var l = lines[idx].Trim();
                            if (string.IsNullOrWhiteSpace(l) || l.StartsWith("#")) { idx++; continue; }
                            if (l.StartsWith("[[")) break;
                            var parts = l.Split('=', 2);
                            if (parts.Length == 2)
                            {
                                dict[parts[0].Trim()] = parts[1].Trim();
                            }
                            idx++;
                        }
                        // build location
                        if (dict.TryGetValue("x", out var sx) && dict.TryGetValue("y", out var sy) && int.TryParse(sx, out var xi) && int.TryParse(sy, out var yi))
                        {
                            if (xi >= 0 && xi < Width && yi >= 0 && yi < Height)
                            {
                                var loc = _grid[xi, yi];
                                if (dict.TryGetValue("name", out var nameRaw)) loc.Name = TrimQuotes(nameRaw);
                                if (dict.TryGetValue("description", out var descRaw)) loc.Description = TrimQuotes(descRaw);
                                if (dict.TryGetValue("type", out var typeRaw))
                                {
                                    var typeStr = TrimQuotes(typeRaw);
                                    if (Enum.TryParse<LocationType>(typeStr, true, out var lt)) loc.Type = lt;
                                }
                                if (dict.TryGetValue("country", out var countryRaw)) loc.CountryName = TrimQuotes(countryRaw);
                                if (dict.TryGetValue("required_level", out var rl) && int.TryParse(rl, out var rlv)) loc.RequiredLevel = rlv;
                                if (dict.TryGetValue("special", out var specialRaw)) { /* store in Description or leave for now */ }
                            }
                        }
                        continue;
                    }
                    idx++;
                }

                // set neighbor coords
                for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    _grid[x, y].North = y > 0 ? (x, y - 1) : null;
                    _grid[x, y].South = y < Height - 1 ? (x, y + 1) : null;
                    _grid[x, y].West = x > 0 ? (x - 1, y) : null;
                    _grid[x, y].East = x < Width - 1 ? (x + 1, y) : null;
                }

                SaveToDb();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadMapsFromFiles failed: {ex.Message}");
            }
        }

        private static string TrimQuotes(string s)
        {
            s = s.Trim();
            if (s.StartsWith("\"") && s.EndsWith("\"")) return s.Substring(1, s.Length - 2);
            return s;
        }

        // Player position persistence and movement
        private PlayerPosition? GetPlayerPositionFromDb(string username)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<PlayerPosition>("playerpositions");
            return col.FindOne(p => p.Username == username);
        }

        private void SavePlayerPositionToDb(PlayerPosition pos)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<PlayerPosition>("playerpositions");
            var existing = col.FindOne(p => p.Username == pos.Username);
            if (existing == null) col.Insert(pos);
            else col.Update(pos);
        }

        public PlayerPosition GetOrCreatePlayerPosition(string username)
        {
            var pos = GetPlayerPositionFromDb(username);
            if (pos != null)
            {
                // reset daily moves if needed
                if (pos.LastResetDate.Date < DateTime.UtcNow.Date)
                {
                    pos.MovesRemaining = 10;
                    pos.LastResetDate = DateTime.UtcNow.Date;
                    SavePlayerPositionToDb(pos);
                }
                return pos;
            }

            // default start: center if grid exists, otherwise (0,0)
            int sx = 0, sy = 0;
            if (_grid != null)
            {
                sx = Math.Clamp(Width / 2, 0, Width - 1);
                sy = Math.Clamp(Height / 2, 0, Height - 1);
            }

            var newPos = new PlayerPosition
            {
                Username = username,
                X = sx,
                Y = sy,
                LastMoveTime = DateTime.UtcNow,
                MovesRemaining = 10,
                LastResetDate = DateTime.UtcNow.Date
            };
            // If user has a country and a capital cell exists for that country, place them there instead
            try
            {
                var us = new UserService();
                var u = us.GetByUsername(username);
                if (u != null && u.CountryId.HasValue)
                {
                    var cs = new CountryService();
                    var country = cs.GetCountryById(u.CountryId.Value);
                    if (country != null)
                    {
                        // find a WorldLocation with CountryName == country.Name and Type == Town (capital preferred)
                        if (_grid == null) LoadFromDb();
                        if (_grid != null)
                        {
                            for (int x = 0; x < Width; x++)
                            for (int y = 0; y < Height; y++)
                            {
                                var cell = _grid[x, y];
                                if (cell != null && cell.CountryName == country.Name && cell.Type == LocationType.Town)
                                {
                                    newPos.X = x; newPos.Y = y;
                                    goto FoundCapital;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
FoundCapital:;
            SavePlayerPositionToDb(newPos);
            return newPos;
        }

        // Move player by direction: "north", "south", "east", "west"
        public MoveResult MovePlayer(string username, string direction)
        {
            var result = new MoveResult { Success = false };
            var pos = GetOrCreatePlayerPosition(username);
            // reset daily moves if needed
            if (pos.LastResetDate.Date < DateTime.UtcNow.Date)
            {
                pos.MovesRemaining = 10;
                pos.LastResetDate = DateTime.UtcNow.Date;
            }

            if (pos.MovesRemaining <= 0)
            {
                result.Message = "移動可能回数が残っていません。";
                result.MovesRemaining = pos.MovesRemaining;
                return result;
            }

            int nx = pos.X, ny = pos.Y;
            switch (direction?.ToLowerInvariant())
            {
                case "north": ny = pos.Y - 1; break;
                case "south": ny = pos.Y + 1; break;
                case "east": nx = pos.X + 1; break;
                case "west": nx = pos.X - 1; break;
                default:
                    result.Message = "無効な方向です。north/south/east/west を指定してください。";
                    result.MovesRemaining = pos.MovesRemaining;
                    return result;
            }

            var loc = GetLocation(nx, ny);
            if (loc == null)
            {
                result.Message = "その方向には移動できません（範囲外）。";
                result.MovesRemaining = pos.MovesRemaining;
                return result;
            }

            // If moving from a Town and the new location is a Field or other area, allow transition
            var currentLoc = GetLocation(pos.X, pos.Y);
            if (currentLoc != null && currentLoc.Type == LocationType.Town && loc.Type != LocationType.Town)
            {
                // transition from town to field (no extra checks here)
            }

            if (!loc.IsAccessible)
            {
                result.Message = "そのエリアにはアクセスできません。";
                result.MovesRemaining = pos.MovesRemaining;
                return result;
            }

            // perform move
            pos.X = nx; pos.Y = ny;
            pos.LastMoveTime = DateTime.UtcNow;
            pos.MovesRemaining = Math.Max(0, pos.MovesRemaining - 1);
            SavePlayerPositionToDb(pos);

            result.Success = true;
            result.Message = $"移動しました: {loc.Name} (x:{nx}, y:{ny})";
            result.NewLocation = loc;
            result.MovesRemaining = pos.MovesRemaining;
            // encounter handling can be added later
            // simple encounter check for Field: if location has Enemies defined, spawn them
            if (loc.Enemies != null && loc.Enemies.Length > 0 && loc.Type == LocationType.Field)
            {
                var ms = new MonsterService();
                var enemies = ms.SpawnEnemiesByNames(loc.Enemies, loc.EnemyLevel);
                result.EncounteredEnemies = enemies.Count > 0;
                result.EncounteredEnemyNames = enemies.Select(e => e.Name).ToArray();
            }
            else
            {
                result.EncounteredEnemies = false;
            }
            return result;
        }

        // Initialize a grid and optionally seed randomized terrain
        public void Initialize(int width, int height, bool seedRandom = true)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            _grid = new WorldLocation[Width, Height];

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                _grid[x, y] = new WorldLocation
                {
                    X = x,
                    Y = y,
                    Name = $"Loc_{x}_{y}",
                    Description = "",
                    Type = LocationType.Field,
                    IsAccessible = true
                };
            }

            if (seedRandom)
                SeedRandomTerrain();

            SaveToDb();
        }

        // Simple randomized terrain seeding
        private void SeedRandomTerrain()
        {
            if (_grid == null) return;
            var rnd = new Random();
            var types = Enum.GetValues(typeof(LocationType)).Cast<LocationType>().ToArray();
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                var t = types[rnd.Next(types.Length)];
                _grid[x, y].Type = t;
                _grid[x, y].Name = t.ToString() + $"_{x}_{y}";
                // set neighbor coordinates
                _grid[x, y].North = y > 0 ? (x, y - 1) : null;
                _grid[x, y].South = y < Height - 1 ? (x, y + 1) : null;
                _grid[x, y].West = x > 0 ? (x - 1, y) : null;
                _grid[x, y].East = x < Width - 1 ? (x + 1, y) : null;
            }
        }

        public WorldLocation? GetLocation(int x, int y)
        {
            if (_grid == null) LoadFromDb();
            if (_grid == null) return null;
            if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
            return _grid[x, y];
        }

        public IEnumerable<WorldLocation> GetNeighbors(int x, int y)
        {
            var loc = GetLocation(x, y);
            if (loc == null) yield break;
            if (loc.North.HasValue) yield return GetLocation(loc.North.Value.X, loc.North.Value.Y)!;
            if (loc.South.HasValue) yield return GetLocation(loc.South.Value.X, loc.South.Value.Y)!;
            if (loc.East.HasValue) yield return GetLocation(loc.East.Value.X, loc.East.Value.Y)!;
            if (loc.West.HasValue) yield return GetLocation(loc.West.Value.X, loc.West.Value.Y)!;
        }

        public IEnumerable<WorldLocation> FindByType(LocationType type)
        {
            if (_grid == null) LoadFromDb();
            if (_grid == null) return Enumerable.Empty<WorldLocation>();
            var list = new List<WorldLocation>();
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (_grid[x, y].Type == type) list.Add(_grid[x, y]);
            }
            return list;
        }

        // Persistence using LiteDB: store each cell as document
        public void SaveToDb()
        {
            if (_grid == null) return;
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<WorldLocation>("worldlocations");
            col.DeleteAll();
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                col.Insert(_grid[x, y]);
            }
        }

        public void LoadFromDb()
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<WorldLocation>("worldlocations");
            var all = col.FindAll().ToList();
            if (!all.Any()) return;
            // infer bounds
            int maxX = all.Max(l => l.X);
            int maxY = all.Max(l => l.Y);
            Width = maxX + 1;
            Height = maxY + 1;
            _grid = new WorldLocation[Width, Height];
            foreach (var l in all)
            {
                if (l.X >= 0 && l.X < Width && l.Y >= 0 && l.Y < Height)
                    _grid[l.X, l.Y] = l;
            }
        }
    }
}
