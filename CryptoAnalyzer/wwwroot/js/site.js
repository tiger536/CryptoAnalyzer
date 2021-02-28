var mySite = {
    addChart: function (chartContainer, chartTitle, series1, series1Name, series2, series2Name) {
        var series = [];
        series.push({
            name: series1Name,
            data: series1,
            lineWidth: 3,
            animation: false,
            shadow: false,
            tooltip: {
                valueDecimals: 2
            }
        });
        if (series2) {
            series.push({
                name: series2Name,
                data: series2,
                lineWidth: 3,
                animation: false,
                shadow: false,
                opacity: 0.5,
                enableMouseTracking: false,
                tooltip: {
                    valueDecimals: 2
                }
            });
        }
        Highcharts.stockChart(chartContainer, {
            chart: {
                animation: false,
                shadow: false
            },
            scrollbar: {
                showFull: false
            },
            rangeSelector: {
                selected: 1
            },
            title: {
                text: chartTitle
            },
            navigator: {
                enabled: false
            },
            time: {
                timezoneOffset: (new Date()).getTimezoneOffset()
            },
            yAxis: {
                opposite: false
            },
            series: series
        });
    },
    addSeriesMACDChart: function (chartContainer, chartTitle, series1, series1Name) {
        Highcharts.stockChart(chartContainer, {
            rangeSelector: {
                selected: 1
            },
            chart: {
                animation: false,
                shadow: false
            },
            scrollbar: {
                showFull: false
            },
            time: {
                timezoneOffset: (new Date()).getTimezoneOffset()
            },
            yAxis: [{
                height: '75%',
                opposite: false,
                resize: {
                    enabled: true
                },
                labels: {
                    align: 'right',
                    x: -3
                },
                title: {
                    text: '$'
                }
            },
            {
                top: '75%',
                opposite: false,
                height: '35%',
                labels: {
                    align: 'right',
                    x: -3
                },
                offset: 0,
                title: {
                    text: 'MACD'
                }
            }],
            xAxis: {
                labels: {
                    align: 'left'
                }
            },
            navigator: {
                enabled: false
            },
            title: {
                text: chartTitle
            },
            series: [{
                lineWidth: 3,
                animation: false,
                shadow: false,
                id: 'price',
                name: series1Name,
                data: series1
            },
            {
                type: 'macd',
                yAxis: 1,
                animation: false,
                linkedTo: 'price',
                signalLine: {
                    styles: {
                        lineColor: '#fc0303'
                    }
                },
                macdLine: {
                    styles: {
                        lineColor: '#001ae6'
                    }
                }            
            }]
        });
    },
    handleSpotlightCheckbox: function (coinID) {
        $("#spotlightCheckbox").change(function () {
            $.ajax({
                url: '/Coins/Spotlight',
                data: { 'important': this.checked, 'coinID': coinID },
                type: "post"
            });
        });
    },
    handleIgnoredCheckbox: function (coinID) {
        $("#ignoredCheckbox").change(function () {
            $.ajax({
                url: '/Coins/Ignore',
                data: { 'ignore': this.checked, 'coinID': coinID },
                type: "post"
            });
        });
    },
    handleFastRefreshCheckbox: function (coinID) {
        $("#fastRefreshCheckbox").change(function () {
            $.ajax({
                url: '/Coins/FastUpdate',
                data: { 'fastUpdate': this.checked, 'coinID': coinID },
                type: "post"
            });
        });
    },
    handleNotesTextbox: function (coinID) {
        $("#btnNotes").click(function () {
            $.ajax({
                url: '/Coins/Notes',
                data: { 'notes': $("#notesTextbox").val(), 'coinID': coinID },
                type: "post"
            });
        });
    }
}