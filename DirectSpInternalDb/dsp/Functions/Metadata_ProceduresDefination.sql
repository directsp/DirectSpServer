CREATE FUNCTION [dsp].[Metadata_ProceduresDefination] ()
RETURNS TABLE
AS
RETURN SELECT	S.name AS SchemaName, O.name AS ObjectName, O.type AS Type, OBJECT_DEFINITION(O.object_id) AS Script
		FROM	sys.objects AS O
				INNER JOIN sys.schemas AS S ON S.schema_id = O.schema_id
		WHERE	O.type IN ( 'FN', 'IF', 'TF', 'P' );