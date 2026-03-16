/*
Beschreibung: Abrufen aller ssis-logeinträge.
Autor: Dimitri Khodak
Erstellungsdatum: 12.12.2024
Datum letzter Änderung: 01.01.2025
*/

SELECT [id]
      ,[event]
      ,[source]
      ,[starttime]
      ,[endtime]
      ,[datacode]
      ,[message]
  FROM [SSISDB].[dbo].[sysssislog]
  ORDER BY starttime desc