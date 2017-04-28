# ITLGMCA_CALL
Asterisk to Medialog connector

Установка 

для работы требуется .NET Framework 3.5

копируем в каталог C:\Program Files (x86)\PMT\Medialog\private\
AsterNET.dll
ITLGMCA_CALL.CONF
ITLGMCA_CALL.exe
ITLGMCA_SQL.jsn
readme.txt
====================
настраиваем параметры подключения к базе SQL, файл ITLGMCA_SQL.jsn 
====================
"MedialogConnectionString":"Provider=sqloledb;Data Source=dev-srv\\sqlexpress;Initial Catalog=medialog_750;User Id=sa;Password=1;"
строка подключения, обычная OLEDB строка, можно подключаться хоть к файлу Excell
====================
"MedialogQuery":"select replace(dbo.PreparePhone(phone,\u0027\u0027,\u0027\u0027),\u0027+7\u0027,\u00278\u0027) as tel, US_CALLS_SOURCES_ID from us_calls_sources"
запрос к базе, одинарная кавычка экранируется \' или кодом \u0027
астериск передает городские телефоны в виде
84951234567
SQL-функция dbo.PreparePhone(phone,'','') приводит любой номер к виду +74951234567
поэтому делается замена '+7' на '8'
====================
"password":"ZjiOgd34JIvlbW+6zEzopA=="
если "password" пустой, то указать пароль в строке подключения открытым текстом
зашифрованный пароль можно получить, если ввести в программе пароль от астериска, SaveOptions, скопировать шифрованную строку из ITLGMCA_CALL.CONF в ITLGMCA_SQL.jsn 
====================
"CallerNameColumn":"US_CALLS_SOURCES_ID"
название поля для MCA_CALL.INI это имя столбца, которое будет в таблице CALLS в базе
====================
"Replase8":false
заменять 8495 1234567 на 7495 1234567
====================
"IgnoreChannel1String":"from-queue"
"IgnoreChannel2String":""
не передавать в медиалог звонки, содержащие строки в полях Channel1 или Channel2
====================
"DebugFile":false
включить сохранение в лог-файл
====================
если в каталоге программы нет конфига, 
то значения по умолчанию заполнятся при нажатии на SaveOptions, аналогично файлу ITLGMCA_CALL.CONF
====================
{"MedialogConnectionString":"Provider=sqloledb;Data Source=dev-srv\\sqlexpress;Initial Catalog=medialog_750;User Id=sa;Password=1;","MedialogQuery":"select replace(dbo.PreparePhone(phone,\u0027\u0027,\u0027\u0027),\u0027+7\u0027,\u00278\u0027) as tel ,system as name from us_calls_sources","password":"","CallerNameColumn":""}
====================
Программа к базе подключается только один раз при запуске, и потом берет значения из памяти.
Если к базе не удалось подключиться, появляется сообщение, и возможна дальнейшая работа. 
Для работы без справочника установите пустые параметры 
"MedialogConnectionString":""
"MedialogQuery":""
===================
при запуске программы из коммандной строки c любым параметром, например
ITLGMCA_CALL.exe ff
в консоль выводится отладка:
Loaded 51 elements
callednum=tulskaja
PHONE=129
CALL_UID=1450807970.91034
CallerId2=907
Channel=
Channel1=IAX2/tulskaja-4025
Channel2=SIP/907-00006198
Reason=
Response=
UniqueId1=1450807970.91034
UniqueId2=1450807970.91035
Server=
callednum=tulskaja
US_CALLS_SOURCES_ID=53
