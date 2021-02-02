CREATE TABLE dbo.CryptoCurrency
(
	Id INT IDENTITY(1,1) PRIMARY KEY,
	Code VARCHAR(50),
	Symbol VARCHAR(10),
	Name VARCHAR(100),
	MarketCapRank INT,
	PublicInterestScore DECIMAL(9,2)
)