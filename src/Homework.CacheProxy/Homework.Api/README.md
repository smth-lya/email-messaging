## Homework.CacheProxy

Проект демонстрирует реализацию паттерна Cache-Aside (Lazy Loading)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791)
![Redis](https://img.shields.io/badge/Redis-8.2-DC382D)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)

### Что сделано
- Паттерн Cache-Aside - при чтении проверяем Redis, при записи инвалидируем кеш
- PostgreSQL - основное хранилище 
- Redis - кэширующий слой
- Docker Compose - запуск всех сервисов
- CRUD API

### Как работает
1. Проверяет Redis по ключу `prefix:{id}`
2. Если данные есть, то возвращает их из кэша
3. Если нет, то идет в PostgreSQL, возвращает результат и прогревает кэш с TTL

![working-cache-example.png](../../../docs/cache-proxy/working-cache-example.png)
*Демонстрация Cache-Aside: первый запрос идет в БД, второй - из Redis*

### Архитектура

![c4-container-diagram.png](../../../docs/cache-proxy/c4-container-diagram.png)

### Некоторые моменты реализации

#### 1. Декоратор для репозитория
`RedisCachedProductRepository` оборачивает `EfProductRepository` и добавляет кэширование - не изменяя логику работы с БД.

#### 2. Инвалидация кэша
При добавлении/обновлении/удалении - кэш сбрасывается (принцип cache-aside):
```csharp
private async Task InvalidateAsync(Guid id)
{
    var key = $"{_settings.KeyPrefix}:{id}";
    await _cache.KeyDeleteAsync(key);
}
```

#### 3. Настройки TTL и префикса
Вынесены в `appsettings.json`:
```json
"ProductCacheSettings": {
  "TTL": 300,
  "KeyPrefix": "product"
}
```

### API Endpoints

| Метод | URL | Описание |
|-------|-----|----------|
| POST | `/product/create` | Создать товар |
| GET | `/product/{id}` | Получить товар (с кэшем) |
| POST | `/product/update` | Обновить товар |
| POST | `/product/delete/{id}` | Удалить товар |

### Запуск проекта

```bash
docker-compose up -d
```

После запуска:
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger