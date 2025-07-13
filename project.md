# 🍽️ Proje: Akıllı Yemek Sipariş Takip API (Dotnet)

## 🎯 Amaç
Yerel restoranlar için, müşterilerin mobil uygulamadan sipariş verebildiği; restoranın siparişi yönetip kuryeye atadığı, sipariş sürecinin yönetildiği RESTful API geliştirilecektir.

---

## ✅ Özellikler
- Dotnet Core Web API
- JWT Authentication
- Swagger dökümantasyonu (Swashbuckle)
- Role-based authorization: Müşteri, Restoran, Kurye
- Restoranlar sadece kendi ürünlerini/siparişlerini yönetebilir

---

## 🔐 Auth Sistemi

### Endpointler:
- `POST /auth/register`
- `POST /auth/login`
- `GET /auth/me`

---

## 🏪 Restoran Yönetimi

### Endpointler:
- `GET /restaurants`  
- `GET /restaurants/{id}`
- `POST /restaurants` *(sadece kayıt olurken)*
- `PUT /restaurants/{id}`

---

## 🍔 Ürün Yönetimi

### Endpointler:
- `GET /products`
- `GET /restaurants/{id}/products`
- `POST /products` *(sadece restoran)*
- `PUT /products/{id}`
- `DELETE /products/{id}`

---

## 🛒 Sipariş Yönetimi

### Endpointler:
- `POST /orders`  
```json
{
  "restaurantId": 1,
  "items": [
    { "productId": 2, "quantity": 1 },
    { "productId": 4, "quantity": 3 }
  ],
  "note": "Acısız olsun"
}
```

- `GET /orders` *(müşteri)*
- `GET /restaurant/orders` *(restoran)*
- `GET /courier/orders` *(kurye)*
- `PATCH /orders/{id}/status`
```json
{ "status": "preparing" }
```
- `PATCH /orders/{id}/assign-courier`
```json
{ "courierId": 8 }
```

---

## 🛵 Kurye Yönetimi

### Endpointler:
- `GET /couriers`
- `POST /couriers`
- `PUT /couriers/{id}`
- `DELETE /couriers/{id}`

---

## 🌟 Opsiyonel Gelişmiş Özellikler
- Tahmini teslim süresi hesaplama
- Siparişe yorum ve puan bırakma (`POST /orders/{id}/rate`)
- Sipariş geçmişi (audit trail)
- Mock kurye lokasyonu (sabit veri ile)

---

## 🔧 Teknolojiler
- .NET 8 Web API
- Entity Framework Core
- Swashbuckle (Swagger UI)
- FluentValidation
- SQL Server veya SQLite
- JWT Authentication

---

## 🧪 Swagger Gereksinimleri
- Her endpoint summary ve description içermeli
- Enum tanımlamaları açıkça gösterilmeli
- JWT ile yetkili istekler test edilebilir olmalı

---

## 📌 Notlar
- Kod yorumları ve endpoint açıklamaları anlaşılır olmalı
- Her kullanıcı sadece kendi verilerine erişebilmelidir
- Swagger üzerinden test edilebilirlik önceliklidir

