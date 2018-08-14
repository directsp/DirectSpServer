CREATE SCHEMA [api]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addextendedproperty @name = N'InvokerApi', @value = N'1', @level0type = N'SCHEMA', @level0name = N'api';

