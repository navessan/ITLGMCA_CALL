# ITLGMCA_CALL
Asterisk to Medialog connector

��������� 

��� ������ ��������� .NET Framework 3.5

�������� � ������� C:\Program Files (x86)\PMT\Medialog\private\
AsterNET.dll
ITLGMCA_CALL.CONF
ITLGMCA_CALL.exe
ITLGMCA_SQL.jsn
readme.txt
====================
����������� ��������� ����������� � ���� SQL, ���� ITLGMCA_SQL.jsn 
====================
"MedialogConnectionString":"Provider=sqloledb;Data Source=dev-srv\\sqlexpress;Initial Catalog=medialog_750;User Id=sa;Password=1;"
������ �����������, ������� OLEDB ������, ����� ������������ ���� � ����� Excell
====================
"MedialogQuery":"select replace(dbo.PreparePhone(phone,\u0027\u0027,\u0027\u0027),\u0027+7\u0027,\u00278\u0027) as tel, US_CALLS_SOURCES_ID from us_calls_sources"
������ � ����, ��������� ������� ������������ \' ��� ����� \u0027
�������� �������� ��������� �������� � ����
84951234567
SQL-������� dbo.PreparePhone(phone,'','') �������� ����� ����� � ���� +74951234567
������� �������� ������ '+7' �� '8'
====================
"password":"ZjiOgd34JIvlbW+6zEzopA=="
���� "password" ������, �� ������� ������ � ������ ����������� �������� �������
������������� ������ ����� ��������, ���� ������ � ��������� ������ �� ���������, SaveOptions, ����������� ����������� ������ �� ITLGMCA_CALL.CONF � ITLGMCA_SQL.jsn 
====================
"CallerNameColumn":"US_CALLS_SOURCES_ID"
�������� ���� ��� MCA_CALL.INI ��� ��� �������, ������� ����� � ������� CALLS � ����
====================
"Replase8":false
�������� 8495 1234567 �� 7495 1234567
====================
"IgnoreChannel1String":"from-queue"
"IgnoreChannel2String":""
�� ���������� � �������� ������, ���������� ������ � ����� Channel1 ��� Channel2
====================
"DebugFile":false
�������� ���������� � ���-����
====================
���� � �������� ��������� ��� �������, 
�� �������� �� ��������� ���������� ��� ������� �� SaveOptions, ���������� ����� ITLGMCA_CALL.CONF
====================
{"MedialogConnectionString":"Provider=sqloledb;Data Source=dev-srv\\sqlexpress;Initial Catalog=medialog_750;User Id=sa;Password=1;","MedialogQuery":"select replace(dbo.PreparePhone(phone,\u0027\u0027,\u0027\u0027),\u0027+7\u0027,\u00278\u0027) as tel ,system as name from us_calls_sources","password":"","CallerNameColumn":""}
====================
��������� � ���� ������������ ������ ���� ��� ��� �������, � ����� ����� �������� �� ������.
���� � ���� �� ������� ������������, ���������� ���������, � �������� ���������� ������. 
��� ������ ��� ����������� ���������� ������ ��������� 
"MedialogConnectionString":""
"MedialogQuery":""
===================
��� ������� ��������� �� ���������� ������ c ����� ����������, ��������
ITLGMCA_CALL.exe ff
� ������� ��������� �������:
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
