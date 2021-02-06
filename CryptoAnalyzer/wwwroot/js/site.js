var mySite = {
    addNewMultiSeriesCanvasChart: function (chartContainer, chartTitle, series1, series1Name, series2, series2Name) {
        var chart = new CanvasJS.Chart(chartContainer, {
            theme: "dark2",
            title: {
                text: chartTitle
            },
            axisY: {
                title: "USD",
                titleFontSize: 24,
                prefix: "$"
            },
            axisX: {
                valueFormatString: "HH:mm",
            },
            toolTip: {
                shared: true
            },
            data: [{
                type: "line",
                color: "#037ffc",
                name: series1Name,
                yValueFormatString: "$#,##0.##",
                xValueFormatString: "HH:mm",
                xValueType: "dateTime",
                dataPoints: series1
            },
            {             
                type: "line",
                color: "green",
                name: series2Name,
                yValueFormatString: "$#,##0.##",
                xValueFormatString: "HH:mm",
                xValueType: "dateTime",
                dataPoints: series2
            }]
        });
chart.render();

    }
}