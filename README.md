# QuestHelper - рабочее название проекта GoSh!
# Цели проекта:
- Дать возможность пользователю отметить интересные места, с фото и текстовым описанием
- Опубликовать собранные данные друзьям для просмотра и совместного редактирования

Небольшое описание в статье:
https://habr.com/ru/post/458272/

Приложение в Google Play:
https://play.google.com/store/apps/details?id=com.sd.gosh&hl=ru

Endpoint backend:
http://igosh.pro/api

## Структура солюшена:
- Проект QuestHelper.Server. Основной проект для сборки сервера на базе WebApi .Net Core 2.1
- Проект QuestHelper.SharedModelsWS. Проект с моделями контрактов, общими для API и клиента.
- Проект QuestHelperServer.Tests. Юнит-тесты для серверной части.

Проекты сборки клиентского приложения вынесены в отдельный репозиторий:
https://github.com/gromozeka07b9/QuestHelper

## Работа с API:
Примеры работы с API находятся в корне репозитория в каталоге Postman в соответствующем формате.

API представлено в виде публичной части и приватной.
* http://igosh.pro/api/v2/public/ - публичные запросы, используются для получения данных сайтом для отображения общедоступных данных.
* http://igosh.pro/api/ - приватные запросы, для получения/обновления/создания данных по личным маршрутам (не опубликованным)

Для выполнения запросов к публичной части API авторизация не требуется.
Для выполнения запросов к личной части требуется авторизоваться, в дальнейшем запросы будут выполняться в контексте учетной записи.
Например, метод создания маршрута приведет к созданию от имени той учетной записи, под которой произошла авторизация.

## Авторизация:
- Первым делом надо завести себе учетку в GoSh!, можно зарегистрировать в приложении (пока что НЕ используя Google!).
- После того, как учетка появится, нужно вызвать метод API POST http://{{hostname}}/api/account
Content-Type: application/json
Body:
{
  "Username": "Имя пользователя в GoSh!",
  "Password": "Пароль, соответственно"
}
В результате запроса получите response, в котором в поле "access_token" будет тот Bearer-токен, который в дальнейшем нужно использовать при каждом запросе к API.
С этим токеном уже можно выполнять запросы к приватной части API.
К примеру, метод GET http://{{hostname}}/api/feed - возвращает ленту маршрутов, доступную конкретному пользователю, который определяется по переданному токену.

## Основы работы с данными:
* Все данные в Gosh версионированы. Это необходимое условие для работы синхронизации. Версия объекта (например маршрута или точки маршрута) увеличивается при каждом изменении объекта. Например, при изменении свойств маршрута в мобильном приложении версия увеличивается, при запуске методов синхронизации с сервером версия проверяется, и если на сервере версия меньше чем на клиенте (в приложении) - маршрут на сервере загружается из приложения. В противном случае, если версия в приложении меньше, чем на сервере - считаем, что маршрут в приложении устарел, и он обновляется данными сервера. Если же версии идентичны - считаем маршрут синхронизированным.
* Версии есть не только у маршрутов (Route), но так же у точки маршрута (RoutePoint), медиаобъекта (RoutePointMediaObject), точки интереса для карты (POI)
* RoutePointMediaObject хранит ссылку на медиа-файл, которым может быть Image, Audio, Video. Для Image на сервере хранится два файла - оригинал и preview, именование в формате:
  * img_GUID.jpg - оригинал
  * img_GUID_preview.jpg - превью

## Последовательность работы с API по маршрутам.
Все примеры находятся в проекте Postman.

**Пример получения публичных маршрутов (Route all в Postman)**
<br>
GET http://{{hostname}}/api/v2/public/routes?pageSize=100&range=[0,9]&filter={"name":"2019"}
В качестве параметров передаем:
* pageSize - размер порции данных
* range - диапазон страниц
* filter - ключ-значение с фильтрами. В примере фильтр по названию маршрута - поиск по like %2019%
Возвращается список объектов Route, удовлетворяющих условию.

**Пример получения приватных маршрутов (Postman private/v2/routes)**
<br>
GET http://{{hostname}}/api/v2/Routes?pageSize=100&range=%5B0%2C99%5D
Параметры и результат аналогичны предидущему примеру.
Для выполнения запроса требуется указать Bearer Token, полученный при авторизации.

**Пример получения точек маршрутов (Postman private/v2/route points)**
<br>
GET http://{{hostname}}/api/v2/routepoints?pageSize=100&range=%5B0%2C99%5D&filter={"routeId":"12341234-1234-4b9d-a151-bece30f6a8c0"}
Параметры аналогичны получению маршрутов - указываем key/value для фильтрации.
В данном примере мы ищем по полю routeId = 12341234-1234-4b9d-a151-bece30f6a8c0, чтобы получить все точки маршрута 12341234-1234-4b9d-a151-bece30f6a8c0
Требуется передача Bearer Token.

**Пример получения списка маршрутов пользователя (Postman /private/v1/sync/route versions)***
<br>
GET http://{{hostname}}/api/route/version/get?onlyPersonal=true
В качестве параметра передается флаг onlyPersonal - true для получения только личных маршрутов, false для получения как личных, так и для маршрутов, где вас добавили как соавтора

**Пример получения полных данных по маршруту (Postman /private/v1/get route)**
<br>
GET http://{{hostname}}/api/route/2354558d-6480-4b9d-a151-bece30f6a8c0
В качестве параметра указываем Id интересующего маршрута
Возвращается структура, описывающая как свойства объекта Route, так и коллекции связанных RoutePoint, так и связанных с ними изображениями RoutePointMediaObject

**Добавление нового маршрута (Postman private/v2/routes)**
<br>
PUT http://{{hostname}}/api/v2/Routes/12341234-1234-4b9d-a151-bece30f6a8c0
Да, для добавления именно PUT. Это ошибка, недосмотр. К сожалению, пока так. Как руки дойдут, будет POST.
Для добавления нового маршрута необходимо вручную сгенерировать GUID для маршрута, например здесь https://www.guidgenerator.com/
GUID передается как в строке запроса, так и в теле запроса в свойстве routeId.
Остальные свойства заполняем по примеру в Postman.
Поле creatorId заполняем в соответствии с Id (GUID) той учетной записи, под которой авторизовались.

**Добавление новой точки в маршрут (Postman private/v2/route point)**
<br>
POST http://{{hostname}}/api/v2/routepoints
Заполняем свойства новой точки в соответствии с примером.
routeId - идентификатор маршрута, к которому точка будет привязана. Например, 12341234-1234-4b9d-a151-bece30f6a8c0 из предидущего примера.
id - идентификатор новой точки. Генерируем новый GUID.

**Обновление точки маршрута**
<br>
Пока не описано, выполняется через метод PUT

**Добавление описания медиа (картинки) в точку маршрута (Postman /private/v1/routePointMediaObject metadata)**
<br>
POST http://{{hostname}}/api/RoutePointMediaObjects
Данный метод добавляет описание картинки к точке маршрута, не саму картинку.
В теле запроса указываем идентификатор картинки (RoutePointMediaObjectId) в виде нового GUID - под этим GUID в дальнейшем будет проходить отправка/получение фото.

**Добавление фото в точку маршрута (Postman /private/v1/routePointMediaObject upload original)**
<br>
POST http://{{hostname}}/api/RoutePointMediaObjects/df4be8d6-9295-43ea-8f23-e54bad885d52/df4be8d6-1234-1234-8f23-e54bad885d52/uploadfile
Данный метод используется для непосредственной загрузки фото на сервер.
Первый параметр в строке запроса (df4be8d6-9295-43ea-8f23-e54bad885d52) - идентификатор точки маршрута.
Второй параметр в строке запроса (df4be8d6-1234-1234-8f23-e54bad885d52) - идентификатор описания медиа, из предидущего примера.
Отправка файла происходит в соответствии с стандартом form-data.
ВАЖНО: имя файла должно соответствовать формату:
* img_GUID.jpg - для оригиналов, где GUID - идентификатор, созданный на предидущем этапе, при добавлении routePointMediaObject (как пример, df4be8d6-1234-1234-8f23-e54bad885d52)
* img_GUID_preview.jpg - для превью, GUID тот же.
Таким образом, одно изображение представлено ДВУМЯ файлами - оригиналом и превью.
Справедливости ради скажу - в GoSh на данный момент оригинал не является совсем исходным файлом, это уменьшенное по качеству изображение, обычно в размер файла около 500-700кб. Для превью размер файла должен быть около 40-50кб.

**Получение с сервера ранее загруженных фото (Postman private/v1/routePointMediaObject)**
<br>
GET http://{{hostname}}/api/RoutePointMediaObjects/df4be8d6-9295-43ea-8f23-e54bad885d52/df4be8d6-1234-1234-8f23-e54bad885d52/img_df4be8d6-1234-1234-8f23-e54bad885d52_preview.jpg
Первый параметр (df4be8d6-9295-43ea-8f23-e54bad885d52) - Id точки маршрута
Второй параметр (df4be8d6-1234-1234-8f23-e54bad885d52) - Id media
Третий параметр - имя файла для скачивания. Правила формирования имени выше описаны.


# ВСЕ GUID в примерах вымышлены, НЕ НУЖНО их использовать, генерируйте свои!
<br>
Буду рад ответить на вопросы, пишите в канал Telegram https://t.me/goshapp или в личку https://t.me/Sergey_Dyachenko
<br>
I will glad to see your questions in Telegram https://t.me/goshapp or in private https://t.me/Sergey_Dyachenko
