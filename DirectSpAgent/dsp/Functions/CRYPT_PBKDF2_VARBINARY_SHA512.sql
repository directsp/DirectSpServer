﻿
-- https://github.com/Anti-weakpasswords/PBKDF2-MSSQL-Custom-A/blob/master/Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512.sql
CREATE FUNCTION [dsp].[CRYPT_PBKDF2_VARBINARY_SHA512]
(
  @Password VARBINARY(MAX), -- HASHBYTES is limited, and HMAC concatenation limits this more, though MAX is a guess
  @Salt VARBINARY(MAX), -- HASHBYTES is limited, and HMAC concatenation limits this more, though MAX is a guess
  @IterationCount INT,
  @Outputbytes INT -- For password hashing, should "naturally" be the digest size (or less) - more than the digest size allows the first <digest size> to remain identical, so someone cracking the PBKDF2'd passwords only needs to generate and check the first <digest size>
  )
RETURNS VARBINARY(MAX)
AS
BEGIN
-- WARNING - if you are using SQL 2012 or better, DO NOT USE Yourfn_CRYPT_PBKDF2_VARBINARY_SHA1 UNLESS YOU NEED BACKWARDS COMPATIBILITY!!!  The 64-bit math required by SHA-512 (and SHA-384) is proportionally faster on CPUs vs. GPU's as of 2013, which reduces a GPU based attacker's advantage.
-- SEE PKCS #5, RFC2898, as well as PBKDF2, i.e. http://tools.ietf.org/rfc/rfc2898.txt
-- WARNING - SQL is NOT a good language for this type of math; results are fairly slow, and are generally better off being implemented by another language.
-- This is a dedicated HMAC-SHA-512 version, with a moderate amount of performance tuning.

/*
SET NOCOUNT ON
DECLARE @Result VARBINARY(64)
DECLARE @start DATETIME2(7)
SET @start = SYSDATETIME()
PRINT 'SHA-512 Test 1 from http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'password'),CONVERT(VARBINARY(MAX),'salt'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x867f70cf1ade02cff3752599a3a53dc4af34c7a669815ae5d513554e1c8cf252c02d470a285a0501bad999bfe943c08f050235d7d68b1da55e63f73b60a57fce THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'SHA-512 Test 2 from http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'password'),CONVERT(VARBINARY(MAX),'salt'),2,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0xe1d9c16aa681708a45f5c7c4e215ceb66e011a2e9f0040713f18aefdb866d53cf76cab2868a39b9f7840edce4fef5a82be67335c77a6068e04112754f27ccf4e THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'SHA-512 Test 3 from http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'password'),CONVERT(VARBINARY(MAX),'salt'),4096,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0xd197b1b33db0143e018b12f3d1d1479e6cdebdcc97c5c0f87f6902e072f457b5143f30602641b3d55cd335988cb36b84376060ecd532e039b742a239434af2d5 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'SHA-512 Test 4 from http://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passwordPASSWORDpassword'),CONVERT(VARBINARY(MAX),'saltSALTsaltSALTsaltSALTsaltSALTsalt'),4096,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x8c0511f4c6e597c6ac6315d8f0362e225f3c501495ba23b868c005174dc4ee71115b59f9e60cd9532fa33e0f75aefe30225c583a186cd82bd4daea9724a3d3b8 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END
PRINT 'Duration (ms): ' + CONVERT(NVARCHAR(23),DATEDIFF(ms,@start,SYSDATETIME()))


PRINT 'Long Test 1a 1 iter Len19pw Len19sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTT'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0xCBE6088AD4359AF42E603C2A33760EF9D4017A7B2AAD10AF46F992C660A0B461ECB0DC2A79C2570941BEA6A08D15D6887E79F32B132E1C134E9525EEDDD744FA THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 1b 100000 iter Len19pw Len19sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTT'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0xACCDCD8798AE5CD85804739015EF2A11E32591B7B7D16F76819B30B0D49D80E1ABEA6C9822B80A1FDFE421E26F5603ECA8A47A64C9A004FB5AF8229F762FF41F THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 2a 1 iter Len20pw Len20sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTl'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x8E5074A9513C1F1512C9B1DF1D8BFFA9D8B4EF9105DFC16681222839560FB63264BED6AABF761F180E912A66E0B53D65EC88F6A1519E14804EBA6DC9DF137007 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 2b 100000 iter Len20pw Len20sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTl'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x594256B0BD4D6C9F21A87F7BA5772A791A10E6110694F44365CD94670E57F1AECD797EF1D1001938719044C7F018026697845EB9AD97D97DE36AB8786AAB5096 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 3a 1 iter Len21pw Len21sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlR'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2P'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0xA6AC8C048A7DFD7B838DA88F22C3FAB5BFF15D7CB8D83A62C6721A8FAF6903EAB6152CB7421026E36F2FFEF661EB4384DC276495C71B5CAB72E1C1A38712E56B THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 3b 100000 iter Len21pw Len21sa- validated against and a Javascript Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlR'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2P'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x94FFC2B1A390B7B8A9E6A44922C330DB2B193ADCF082EECD06057197F35931A9D0EC0EE5C660744B50B61F23119B847E658D179A914807F4B8AB8EB9505AF065 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 4a 1 iter Len63pw Len63sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE5'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJe'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0xE2CCC7827F1DD7C33041A98906A8FD7BAE1920A55FCB8F831683F14F1C3979351CB868717E5AB342D9A11ACF0B12D3283931D609B06602DA33F8377D1F1F9902 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 4b 100000 iter Len63pw Len63sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE5'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJe'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x07447401C85766E4AED583DE2E6BF5A675EABE4F3618281C95616F4FC1FDFE6ECBC1C3982789D4FD941D6584EF534A78BD37AE02555D9455E8F089FDB4DFB6BB THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 5a 1 iter Len64pw Len64sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJem'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0xB029A551117FF36977F283F579DC7065B352266EA243BDD3F920F24D4D141ED8B6E02D96E2D3BDFB76F8D77BA8F4BB548996AD85BB6F11D01A015CE518F9A717 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 5b 100000 iter Len64pw Len64sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJem'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x31F5CC83ED0E948C05A15735D818703AAA7BFF3F09F5169CAF5DBA6602A05A4D5CFF5553D42E82E40516D6DC157B8DAEAE61D3FEA456D964CB2F7F9A63BBBDB5 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 6a 1 iter Len65pw Len65sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57U'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemk'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x28B8A9F644D6800612197BB74DF460272E2276DE8CC07AC4897AC24DBC6EB77499FCAF97415244D9A29DA83FC347D09A5DBCFD6BD63FF6E410803DCA8A900AB6 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 6b 100000 iter Len65pw Len65sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57U'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemk'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x056BC9072A356B7D4DA60DD66F5968C2CAA375C0220EDA6B47EF8E8D105ED68B44185FE9003FBBA49E2C84240C9E8FD3F5B2F4F6512FD936450253DB37D10028 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 7a 1 iter Len127pw Len127sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi0'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x16226C85E4F8D604573008BFE61C10B6947B53990450612DD4A3077F7DEE2116229E68EFD1DF6D73BD3C6D07567790EEA1E8B2AE9A1B046BE593847D9441A1B7 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 7b 100000 iter Len127pw Len127sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi0'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x70CF39F14C4CAF3C81FA288FB46C1DB52D19F72722F7BC84F040676D3371C89C11C50F69BCFBC3ACB0AB9E92E4EF622727A916219554B2FA121BEDDA97FF3332 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 8a 1 iter Len128pw Len128sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x880C58C316D3A5B9F05977AB9C60C10ABEEBFAD5CE89CAE62905C1C4F80A0A098D82F95321A6220F8AECCFB45CE6107140899E8D655306AE6396553E2851376C THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 8b 100000 iter Len128pw Len128sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x2668B71B3CA56136B5E87F30E098F6B4371CB5ED95537C7A073DAC30A2D5BE52756ADF5BB2F4320CB11C4E16B24965A9C790DEF0CBC62906920B4F2EB84D1D4A THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 9a 1 iter Len129pw Len129sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04U'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6P'),1,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x93B9BA8283CC17D50EF3B44820828A258A996DE258225D24FB59990A6D0DE82DFB3FE2AC201952100E4CC8F06D883A9131419C0F6F5A6ECB8EC821545F14ADF1 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END

PRINT 'Long Test 9b 100000 iter Len129pw Len129sa- validated against a Python implemenation of PBKDF2'
SET @Result = YourDB.dbo.Yourfn_CRYPT_PBKDF2_VARBINARY_SHA512(CONVERT(VARBINARY(MAX),'passDATAb00AB7YxDTTlRH2dqxDx19GDxDV1zFMz7E6QVqKIzwOtMnlxQLttpE57Un4u12D2YD7oOPpiEvCDYvntXEe4NNPLCnGGeJArbYDEu6xDoCfWH6kbuV6awi04U'),CONVERT(VARBINARY(MAX),'saltKEYbcTcXHCBxtjD2PnBh44AIQ6XUOCESOhXpEp3HrcGMwbjzQKMSaf63IJemkURWoqHusIeVB8Il91NjiCGQacPUu9qTFaShLbKG0Yj4RCMV56WPj7E14EMpbxy6P'),100000,64)
SELECT @Result
PRINT CASE WHEN @Result IS NULL THEN 'NULL - BAD ALGO?' WHEN @Result = 0x2575B485AFDF37C260B8F3386D33A60ED929993C9D48AC516EC66B87E06BE54ADE7E7C8CB3417C81603B080A8EEFC56072811129737CED96236B9364E22CE3A5 THEN 'PASS' ELSE 'FAIL INVALID RESULT' END
PRINT 'Duration (ms): ' + CONVERT(NVARCHAR(23),DATEDIFF(ms,@start,SYSDATETIME()))

*/

  DECLARE @NumDigestSizesRequiredToEncompassOutputbytes INT
  DECLARE @RemainderOutputbytesAfterNumDigestSizesMinusOne INT
  DECLARE @Working BINARY(64) -- digest size
  DECLARE @ThisIterationResult BINARY(64) -- digest size
  DECLARE @FirstIterationDefinedResult VARBINARY(MAX) -- Salt size + INT size per HMAC definition
  DECLARE @output VARBINARY(MAX)
  DECLARE @CurrentDigestSizeChunk INT
  DECLARE @CurrentIteration INT
  -- Start Inlined HMAC-SHA-512 variables
  DECLARE @ipadRFC2104 BIGINT
  DECLARE @opadRFC2104 BIGINT
  DECLARE @k_ipadRFC2104 BINARY(128) -- BLOCKSIZE in bytes per HMAC definition
  DECLARE @k_opadRFC2104 BINARY(128) -- BLOCKSIZE in bytes per HMAC definition
  --SQL fails to allow binary operations on two binary data types!!!  We use bigint and iterate 8 times for 512 bits = 64 byte blocksize for better performance.
  SET @ipadRFC2104 = CAST(0x3636363636363636 AS BIGINT)
  SET @opadRFC2104 = CAST(0x5C5C5C5C5C5C5C5C AS BIGINT)
  -- End Inlined HMAC-SHA-512 variables  

  SET @NumDigestSizesRequiredToEncompassOutputbytes = (@Outputbytes + 63)/64 -- number > 1 is digest size/digest size minus 1
  SET @RemainderOutputbytesAfterNumDigestSizesMinusOne = @Outputbytes - (@NumDigestSizesRequiredToEncompassOutputbytes - 1) * 64 -- the number in here that's > 1 is the digest size


  SET @output = 0x
  SET @CurrentDigestSizeChunk = 1

  WHILE @CurrentDigestSizeChunk <= @NumDigestSizesRequiredToEncompassOutputbytes
  BEGIN
    SET @FirstIterationDefinedResult = @Salt + CAST(@CurrentDigestSizeChunk AS VARBINARY(4))
    --SET @ThisIterationResult = YourDB.dbo.Yourfn_CRYPT_HMAC_SHA512(@Password,@FirstIterationDefinedResult)

    -- NOTE: Inlining HMAC-SHA-512 appears to improve performance by a factor of six or so.  Setting the HMAC as an Inlined Table Valued Function instead of a Scalar function would reduce this disparity, of course.
    -- START INLINED HMAC-SHA-512 for performance improvement
    -- B = BLOCKSIZE (64 bytes for MD5, SHA-224, SHA-256, and 128 bytes for SHA-384 and SHA-512, per RFC2104 and RFC4868)
    IF LEN(@Password) > 128 -- Applications that use keys longer than B bytes will first hash the key using H and then use the resultant L byte string as the actual key to HMAC 
      SET @Password = CAST(HASHBYTES('SHA2_512', @Password) AS BINARY (128))
    ELSE
      SET @Password = CAST(@Password AS BINARY (128)) -- append zeros to the end of K to create a B byte string

    -- Loop unrolled for definite performance benefit
    -- Must XOR BLOCKSIZE bytes
    SET @k_ipadRFC2104 = CONVERT(BINARY(8),(SUBSTRING(@Password, 1, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 9, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 17, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 25, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 33, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 41, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 49, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 57, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 65, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 73, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 81, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 89, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 97, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 105, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 113, 8) ^ @ipadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 121, 8) ^ @ipadRFC2104))

    -- Loop unrolled for definite performance benefit
    -- Must XOR BLOCKSIZE bytes
    SET @k_opadRFC2104 = CONVERT(BINARY(8),(SUBSTRING(@Password, 1, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 9, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 17, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 25, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 33, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 41, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 49, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 57, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 65, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 73, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 81, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 89, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 97, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 105, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 113, 8) ^ @opadRFC2104))
      + CONVERT(BINARY(8),(SUBSTRING(@Password, 121, 8) ^ @opadRFC2104))

    SET @ThisIterationResult = HASHBYTES('SHA2_512', @k_opadRFC2104 + HASHBYTES('SHA2_512', @k_ipadRFC2104 + @FirstIterationDefinedResult))
    -- END   INLINED HMAC-SHA-512 for performance improvement

    SET @Working = @ThisIterationResult

    SET @CurrentIteration = 1
    WHILE @CurrentIteration < @IterationCount
    BEGIN
      --SET @ThisIterationResult = YourDB.dbo.Yourfn_CRYPT_HMAC_SHA512(@Password,@ThisIterationResult)

      -- NOTE: Inlining HMAC-SHA-512 appears to improve performance by a factor of six or so.  Setting the HMAC as an Inlined Table Valued Function instead of a Scalar function would reduce this disparity, of course.
      -- START INLINED HMAC-SHA-512 for performance improvement
      -- B = BLOCKSIZE (64 bytes for MD5, SHA-224, SHA-256, and 128 bytes for SHA-384 and SHA-512, per RFC2104 and RFC4868)

      -- We've already hashed the password if we needed to!
      -- We've already generated @k_ipadRFC2104 and @k_opadRFC2104 both!

      SET @ThisIterationResult =  HASHBYTES('SHA2_512', @k_opadRFC2104 + HASHBYTES('SHA2_512', @k_ipadRFC2104 + @ThisIterationResult))
      -- END   INLINED HMAC-SHA-512 for performance improvement

      -- Loop unrolled for possible performance benefit
      -- Stupid conversion required because SQL Server can't do binary operations on two binary variables!!!
      -- Must XOR digest size bytes
      SET @Working = CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,1,8)))^SUBSTRING(@Working,1,8)))
        + CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,9,8)))^SUBSTRING(@Working,9,8)))
        + CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,17,8)))^SUBSTRING(@Working,17,8)))
        + CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,25,8)))^SUBSTRING(@Working,25,8)))
        + CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,33,8)))^SUBSTRING(@Working,33,8)))
        + CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,41,8)))^SUBSTRING(@Working,41,8)))
        + CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,49,8)))^SUBSTRING(@Working,49,8)))
        + CONVERT(BINARY(8),(CONVERT(BIGINT,(SUBSTRING(@ThisIterationResult,57,8)))^SUBSTRING(@Working,57,8)))

      SET @CurrentIteration += 1 -- SHA-512 is a SQL 2012 only function, so SQL 2008 only syntax doesn't limit compatibility any further.
    END -- WHILE @CurrentIteration rounds

    SELECT @output = @output +
      CASE
        WHEN @CurrentDigestSizeChunk = @NumDigestSizesRequiredToEncompassOutputbytes THEN CONVERT(BINARY(64),LEFT(@Working,@RemainderOutputbytesAfterNumDigestSizesMinusOne)) -- digest size in bytes
        ELSE CONVERT(BINARY(64),@Working) -- digest size in bytes
        END 
    SET @CurrentDigestSizeChunk += 1 -- SHA-512 is a SQL 2012 only function, so SQL 2008 only syntax doesn't limit compatibility any further.
  END

  RETURN @output

END