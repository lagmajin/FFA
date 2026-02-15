window.fetchSignIn = async function (url, payload) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'same-origin',
    body: JSON.stringify(payload)
  });
  return res.ok;
}

window.fetchGet = async function (url) {
  const res = await fetch(url, { credentials: 'same-origin' });
  if (!res.ok) return null;
  return res.json();
}

window.fetchPostJson = async function (url, payload) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'same-origin',
    body: JSON.stringify(payload)
  });
  if (!res.ok) return null;
  return res.json();
}

window.fetchSignOut = async function (url) {
  const res = await fetch(url, {
    method: 'POST',
    credentials: 'same-origin'
  });
  return res.ok;
}
