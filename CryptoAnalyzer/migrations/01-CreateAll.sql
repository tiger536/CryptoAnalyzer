﻿CREATE TABLE dbo.CryptoCurrency
(
	Id INT IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(50),
	Symbol VARCHAR(10),
	Name VARCHAR(100),
	MarketCapRank INT
)


CREATE TABLE dbo.CryptoDetails
(
    Id           INT IDENTITY(1,1) NOT NULL,
    CoinId       INT NOT NULL,
	LogDate      DATETIMEOFFSET NOT NULL,
	Volume       DECIMAL(15,2) NOT NULL,
	Price        DECIMAL(15,2) NOT NULL,
	MarketCap    DECIMAL(15,2) NOT NULL,
	CONSTRAINT [PK_dbo_CryptoDetails] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, DATA_COMPRESSION = PAGE) ON [PRIMARY]
)
CREATE NONCLUSTERED INDEX [IX_dbo_CryptoDetails_LogDate] ON dbo.CryptoDetails
(
	[LogDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, DATA_COMPRESSION = PAGE) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_dbo_CryptoDetails_CoinId] ON dbo.CryptoDetails
(
	[CoinId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, DATA_COMPRESSION = PAGE) ON [PRIMARY]


CREATE TYPE [dbo].[TVP_CryptoData] AS TABLE(
    Volume       DECIMAL(15,2) NOT NULL,
	Price        DECIMAL(15,2) NOT NULL,
	MarketCap    DECIMAL(15,2) NOT NULL
)
