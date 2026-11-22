# 🧪 TEST SCENARIJI - Autentikacija i Autorizacija

## 📖 Struktura Testiranja

1. **MVC Login/Logout** - Web pregledniku
2. **MVC Create/Edit/Delete** - Autorizacija
3. **API Login** - JWT token
4. **API CRUD Operations** - Role-based auth

---

## Scenario 1: Anonimni Korisnik

### Što može vidjeti:
```
✅ https://localhost:7001/                    → Početna stranica
✅ https://localhost:7001/vjezbe              → Lista vježbi (Index)
✅ https://localhost:7001/vjezbe/1            → Detalji vježbe
✅ https://localhost:7001/planovi             → Lista planova
✅ https://localhost:7001/treninzi            → Lista treninga
✅ https://localhost:7001/korisnici           → Lista korisnika
```

### Što NIJE dozvoljeno:
```
❌ https://localhost:7001/vjezbe/create       → Preusmjeravanje na login
❌ https://localhost:7001/vjezbe/1/edit       → Preusmjeravanje na login
❌ https://localhost:7001/vjezbe/1/delete     → Preusmjeravanje na login
❌ POST /api/vjezbe                           → 401 Unauthorized
❌ PUT /api/vjezbe/1                          → 401 Unauthorized
❌ DELETE /api/vjezbe/1                       → 401 Unauthorized
```

### Akcije:
1. Otvori `https://localhost:7001/Identity/Account/Register`
2. Unesi:
   - Email: `korisnik@test.hr`
   - Lozinka: `Test@12345`
   - OIB: `12345678901`
   - JMBG: `1234567890123`
3. Klikni **Registriraj se**
4. Osiguraj se da je prijavljen → "Odjava" dugme vidljivo

---

## Scenario 2: User Rola (Obični Korisnik)

### Test Korisnik
```
Email: nova-registracija@test.hr
Lozinka: Lozinka@123
Uloga: User (automatski dodijeljena)
```

### Što može raditi:

#### MVC - Pregledavanje (SVE)
```
✅ GET /vjezbe                    → Sve vježbe
✅ GET /vjezbe/1                  → Detalji vježbe
✅ GET /planovi                   → Svi planovi
✅ GET /korisnici                 → Svi korisnici
```

#### MVC - Kreiranje (OGRANIČENO)
```
✅ GET /treninzi/create           → Može kreirati svoj trening
✅ POST /treninzi                 → Kreiraj trening
✅ GET /mjerenja/create           → Može unijeti mjerenje
✅ POST /mjerenja                 → Unesi mjerenje
✅ GET /ciljevi/create            → Može postaviti cilj
✅ POST /ciljevi                  → Postavi cilj
```

#### MVC - Uređivanje
```
✅ GET /treninzi/1/edit           → Samo svojeg treninga
✅ PUT /treninzi/1                → Samo svojeg treninga
```

#### MVC - Brisanje
```
✅ DELETE /treninzi/1             → Samo svojeg treninga
```

#### MVC - NIJE dozvoljeno
```
❌ GET /vjezbe/create             → 403 Forbidden
❌ POST /vjezbe                   → 403 Forbidden
❌ GET /planovi/create            → 403 Forbidden
❌ POST /planovi                  → 403 Forbidden
```

#### API - GET (sve sa JWT tokenima)
```
curl -H "Authorization: Bearer [USER_TOKEN]" \
  https://localhost:7001/api/vjezbe
→ 200 OK - Lista vježbi

curl -H "Authorization: Bearer [USER_TOKEN]" \
  https://localhost:7001/api/planovi
→ 200 OK - Lista planova

curl -H "Authorization: Bearer [USER_TOKEN]" \
  https://localhost:7001/api/korisnici
→ 200 OK - Lista korisnika
```

#### API - POST (samo određeni resursi)
```
✅ POST /api/treninzi
   [Authorize(Roles = "Admin,Trainer,User")]
   → 201 Created

✅ POST /api/mjerenja
   [Authorize(Roles = "Admin,Trainer,User")]
   → 201 Created

✅ POST /api/ciljevi
   [Authorize(Roles = "Admin,Trainer,User")]
   → 201 Created

❌ POST /api/vjezbe
   [Authorize(Roles = "Admin,Trainer")]
   → 403 Forbidden
```

---

## Scenario 3: Trainer Rola

### Test Korisnik (Seed podatak)
```
Email: trainer@teretana.hr
Lozinka: Trainer@123456
Uloga: Trainer
```

### Što može raditi:

#### MVC - Sve kao User PLUS:
```
✅ GET /vjezbe/create             → Kreiranje vježbi
✅ POST /vjezbe                   → Kreiraj vježbu
✅ GET /vjezbe/1/edit             → Uređivanje vježbi
✅ PUT /vjezbe/1                  → Ažuriraj vježbu
✅ DELETE /vjezbe/1               → Obriši vježbu

✅ GET /planovi/create            → Kreiranje planova
✅ POST /planovi                  → Kreiraj plan
✅ GET /planovi/1/edit            → Uređivanje planova
✅ PUT /planovi/1                 → Ažuriraj plan
✅ DELETE /planovi/1              → Obriši plan

✅ GET /korisnici/create          → Kreiranje korisnika
✅ POST /korisnici                → Kreiraj korisnika
```

#### API - POST sa Trainer ulogom
```
✅ POST /api/vjezbe
   [Authorize(Roles = "Admin,Trainer")]
   → 201 Created

✅ POST /api/planovi
   [Authorize(Roles = "Admin,Trainer")]
   → 201 Created

✅ POST /api/korisnici
   [Authorize(Roles = "Admin,Trainer")]
   → 201 Created

✅ POST /api/treninzi
   [Authorize(Roles = "Admin,Trainer,User")]
   → 201 Created
```

#### API - PUT sa Trainer ulogom
```
✅ PUT /api/vjezbe/1
   [Authorize(Roles = "Admin,Trainer")]
   → 204 No Content

✅ PUT /api/planovi/1
   [Authorize(Roles = "Admin,Trainer")]
   → 204 No Content
```

#### API - DELETE sa Trainer ulogom
```
✅ DELETE /api/vjezbe/1
   [Authorize(Roles = "Admin,Trainer")]
   → 204 No Content

✅ DELETE /api/planovi/1
   [Authorize(Roles = "Admin,Trainer")]
   → 204 No Content
```

---

## Scenario 4: Admin Rola

### Test Korisnik (Seed podatak)
```
Email: admin@teretana.hr
Lozinka: Admin@123456
Uloga: Admin
```

### Što može raditi:

#### MVC - SVE AKCIJE
```
✅ Svi GET endpointi
✅ Svi POST endpointi (Create)
✅ Svi PUT endpointi (Edit)
✅ Svi DELETE endpointi (Delete)
```

#### API - SVE AKCIJE
```
✅ POST /api/vjezbe              → 201 Created
✅ PUT /api/vjezbe/1             → 204 No Content
✅ DELETE /api/vjezbe/1          → 204 No Content

✅ POST /api/planovi             → 201 Created
✅ PUT /api/planovi/1            → 204 No Content
✅ DELETE /api/planovi/1         → 204 No Content

✅ POST /api/korisnici           → 201 Created
✅ PUT /api/korisnici/1          → 204 No Content
✅ DELETE /api/korisnici/1       → 204 No Content

✅ POST /api/treninzi            → 201 Created
✅ PUT /api/treninzi/1           → 204 No Content
✅ DELETE /api/treninzi/1        → 204 No Content

✅ POST /api/mjerenja            → 201 Created
✅ PUT /api/mjerenja/1           → 204 No Content
✅ DELETE /api/mjerenja/1        → 204 No Content

✅ POST /api/ciljevi             → 201 Created
✅ PUT /api/ciljevi/1            → 204 No Content
✅ DELETE /api/ciljevi/1         → 204 No Content
```

---

## 🔑 API Test Primjeri (Postman)

### 1. Login i Dobivanje JWT Tokena

**POST** `https://localhost:7001/api/auth/login`

**Headers:**
```
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "email": "admin@teretana.hr",
  "password": "Admin@123456"
}
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sLnNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQHRlcmV0YW5hLmhyIiwiaHR0cDovL3NjaGVtYXMueG1sLnNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbkB0ZXJldGFuYS5nciIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIn0.abc123...",
  "user": {
    "id": "user-id-123",
    "email": "admin@teretana.hr",
    "userName": "admin@teretana.hr",
    "roles": ["Admin"]
  }
}
```

### 2. Korištenje JWT Tokena za Pristup API-ju

**GET** `https://localhost:7001/api/vjezbe`

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiw...
Content-Type: application/json
```

**Response:**
```json
[
  {
    "id": 1,
    "naziv": "Sklekovi",
    "opis": "Vježba za prsne mišiće",
    "misicnaSkupina": "Prsni mišići",
    "videoUrl": "https://youtube.com/...",
    "razinaTezine": 3,
    "potrebnaOprema": false
  },
  ...
]
```

### 3. Kreiranje Vježbe sa Trainer Ulogom

**POST** `https://localhost:7001/api/vjezbe`

**Headers:**
```
Authorization: Bearer [TRAINER_TOKEN]
Content-Type: application/json
```

**Body:**
```json
{
  "naziv": "Novi sklekovi",
  "opis": "Napredna verzija",
  "misicnaSkupina": 1,
  "razinaTezine": 4,
  "potrebnaOprema": true
}
```

**Response:**
```json
{
  "id": 2,
  "naziv": "Novi sklekovi",
  "opis": "Napredna verzija",
  "misicnaSkupina": "Prsni mišići",
  "razinaTezine": 4,
  "potrebnaOprema": true
}
```

### 4. Test - Pokušaj Brisanja kao User (Trebalo bi biti Forbidden)

**DELETE** `https://localhost:7001/api/vjezbe/1`

**Headers:**
```
Authorization: Bearer [USER_TOKEN]
```

**Response:**
```
HTTP 403 Forbidden

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Access denied"
}
```

---

## 🧪 Postman Collection Setup

1. **Kreiraj Collection:** "Teretana API"
2. **Tab: Authorization**
   - Type: `Bearer Token`
   - Token: `{{jwt_token}}`

3. **Pre-request Script:**
```javascript
// Automatski refresh tokena ako je istekao
if (!pm.environment.get("jwt_token") || 
    new Date() > new Date(pm.environment.get("token_expires"))) {
    
    var loginReq = {
        url: pm.environment.get("base_url") + "/api/auth/login",
        method: "POST",
        header: { "Content-Type": "application/json" },
        body: {
            mode: "raw",
            raw: JSON.stringify({
                email: "admin@teretana.hr",
                password: "Admin@123456"
            })
        }
    };

    pm.sendRequest(loginReq, function (err, response) {
        var jsonData = response.json();
        pm.environment.set("jwt_token", jsonData.token);
    });
}
```

4. **Environment Variables:**
```
base_url = https://localhost:7001
admin_email = admin@teretana.hr
admin_password = Admin@123456
trainer_email = trainer@teretana.hr
trainer_password = Trainer@123456
jwt_token = (postavlja se automatski)
```

---

## ✅ Checklist za Verifikaciju

### Autentikacija
- [ ] Registracija novog korisnika sa OIB/JMBG
- [ ] Login sa local accountom
- [ ] Logout i brisanje sessiona
- [ ] Login sa Admin ulogom
- [ ] Login sa Trainer ulogom
- [ ] Login sa User ulogom

### MVC Autorizacija
- [ ] Anonimni korisnik može vidjeti Index
- [ ] Anonimni korisnik ne može vidjeti Create
- [ ] User može vidjeti Create treninga
- [ ] Trainer može vidjeti Create vježbi
- [ ] Admin može vidjeti sve Create

### API Autorizacija
- [ ] JWT login vraća token
- [ ] GET bez tokena vraća 401
- [ ] GET sa tokenomvraća podatke
- [ ] POST kao User vraća 403 za vjezbe
- [ ] POST kao Trainer vraća 201 za vjezbe
- [ ] POST kao Admin vraća 201 za sve

---

## 🐛 Troubleshooting

### Problem: 401 Unauthorized na API
**Rješenje:** Provjeri je li JWT token ispravan i nije istekao (24 sata)

### Problem: 403 Forbidden na API
**Rješenje:** Provjeri je li uloga korisnika dopuštena za tu akciju

### Problem: Redirectujem se na login pri Create
**Rješenje:** Trebam se prijaviti - korisnik nije autentificiran

### Problem: "OIB smije sadržavati samo brojeve"
**Rješenje:** OIB mora biti točno 11 brojeva, npr. `12345678901`

### Problem: "JMBG smije sadržavati samo brojeve"
**Rješenje:** JMBG mora biti točno 13 brojeva, npr. `1234567890123`

