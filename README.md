# CryptoAnalyzer

## Overview 
This is an all round dashboard to grab and display cryptocurrencies info.
In particular it grabs:
- The latest coin added on CoinGecko every hour
- The latest defi project added on Uniswap (and PancakeSwap too when they will fix their GraphQL endpoint)
- Coin value and trading volume from CoinGecko every 5 minutes if set under the ```Spotlight```, or every minute from CoinMarketCap if set as ```Fast Update```.
Every other coin is grabber on a low priority and best effort basis since Coingecko heavily throttle your HTTP calls.
- Number of posts made on /biz as a way to gauche people interest in a particular coin.

The homepage will look like this:
![image](https://user-images.githubusercontent.com/30831275/142732092-95070701-8a23-46d2-9715-99f14c45fe06.png)

![image](https://user-images.githubusercontent.com/30831275/142732181-dae35828-655d-4d4d-a9a1-276e99b9577e.png)

While the coin detail page:
![image](https://user-images.githubusercontent.com/30831275/142732212-4c066e94-09cf-404e-98f5-dd0101582ffb.png)
A chart for OBV, volume comparison, daily posts comparison and price comparison is also present in this page.

## Installation

To run this you will need:
- A SQL Server database to store data. To create the required tables just run the sql scripts in the migrations folder
- An API key to grab data from CoinMarketCap (optional)
- A telegram bot API key (optional)

Everything can be setup in the app settings
