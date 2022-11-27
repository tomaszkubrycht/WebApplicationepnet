var Json;

function insertelev(type,val){
    var data1={type:type,val:val};
    $.ajax({
        url: "/home/Revert",
        dataType: "json",
        method: "GET",
        contenttype:("application/json"),
        data: data1,
        success: function(data){
            $("#elevation").text(" Elevation:"+data+"m").css('font-weight','bold');
        },
        error:function(){return alert("Blad")}
    });
}
function tomasz1(d) {
    $('#here_table').empty();
    $('#elevation').empty()
    $('#general').empty();
    var results = [];
    results = Json.nodesresults.filter(obj => obj.name === d.name);
    resultsnet=Json.network.nodes.filter(obj=>obj.name===d.name);
    var m = results.length;
    var descr=insertelev("ELEVATION", resultsnet[0].elevation);
    $( "#general" ).text( "Node Nr: "+ resultsnet[0].name).css('font-weight','bold');
    var table = $('<table class="table table-striped"></table>');
    //var header = $('<th></th>');
    var headercol1 = $('<th><th>').text('flow');
    var headercol2 = $('<th><th>').text('velocity');
    table.append('<th><tr><td>TimeStep</td><td>Head</td><td>Demand</td><td>Pressure</td></tr></th>');
    //table.append(headercol2);
    //table.append(header);
    for (n = 0; n < m; n++) {
        var row = $('<tr><td>' + results[n].timestep.step + '</td><td>' + (Math.round(results[n].head * 10000) / 10000).toFixed(4) + '</td><td>' + (Math.round(results[n].demand * 10000) / 10000).toFixed(4) + '</td><td>' + (Math.round(results[n].pressure * 10000) / 10000).toFixed(4) + '</td></tr>');
        table.append(row);
        //row.append(row1);
    }
    $('#here_table').append(table);
}
function createlabelarray(data) {
    var labelsnodes=[];
    var xcoord=[];
    var ycoord=[];
    var nodes=data.nodes;
    nodes.forEach(function (nodes) {
        xcoord.push(parseFloat(nodes.coordinate.x));
        ycoord.push(parseFloat(nodes.coordinate.y));
    });
    var maxx = Math.max.apply(Math, xcoord);
    let minx = Math.min.apply(Math, xcoord);
    let maxy = Math.max.apply(Math, ycoord);
    let miny = Math.min.apply(Math, ycoord);
    var linearScaleX = d3.scaleLinear()
        .domain([minx, maxx])
        .range([0, 600]);
    var linearScaleY = d3.scaleLinear()
        .domain([miny, maxy])
        .range([0, 600]);

    for(n=0;n<data.nodes.length;n++){
        labelsnodes.push({x:linearScaleX(nodes[n].coordinate.x),y:linearScaleY(nodes[n].coordinate.y),name:nodes[n].name, width: width, height: height});
        //  arrownodes.push({x:linearScaleX(data.nodes[n].coordinate.x),y:linearScaleX(data.nodes[n].coordinate.y),r:3});
    }
    return labelsnodes;
}
function createanchor(data){
    var arrownodes=[];
    var Json;
    var xcoord=[];
    var ycoord=[];
    var nodes=data.nodes;
    nodes.forEach(function (nodes) {
        xcoord.push(parseFloat(nodes.coordinate.x));
        ycoord.push(parseFloat(nodes.coordinate.y));
    });
    var maxx = Math.max.apply(Math, xcoord);
    let minx = Math.min.apply(Math, xcoord);
    let maxy = Math.max.apply(Math, ycoord);
    let miny = Math.min.apply(Math, ycoord);
    var linearScaleX = d3.scaleLinear()
        .domain([minx, maxx])
        .range([0, 600]);
    var linearScaleY = d3.scaleLinear()
        .domain([miny, maxy])
        .range([0, 600]);

    for(n=0;n<data.nodes.length;n++){
        arrownodes.push({x:linearScaleX(nodes[n].coordinate.x),y:linearScaleY(nodes[n].coordinate.y),r:2});
        //  arrownodes.push({x:linearScaleX(data.nodes[n].coordinate.x),y:linearScaleX(data.nodes[n].coordinate.y),r:3});
    }
    return arrownodes;
}
function createlabels(network) {
    var anchor_array = [],
        label_array = [];
    var anchor_data, labels, circ, links, bounds;
        offset = 20;
        radius = 3;


    start(network);
    redrawLabels();
  
// Functions

    function redrawLabels() {
        // Redraw labels and leader lines

        labels
            .transition()
            .duration(800)
            .attr("x", function (d) {
                return (d.x);
            })
            .attr("y", function (d) {
                return (d.y);
            });
           

        links
            .transition()
            .duration(800)
            .attr("x2", function (d) {
                return (d.x);
            })
            .attr("y2", function (d) {
                return (d.y);
            });
    }

   

    
// Start button function
    function start(data) {
        
        var labelsnodes=[];
        var createanchor1=[];
        labelsnodes=createlabelarray(data);
        createanchor1=createanchor(data);
        svg.selectAll(".dot").data([]).exit().remove();
        svg.selectAll(".label").data([]).exit().remove();
        svg.selectAll(".circ").data([]).exit().remove();
        svg.selectAll(".link").data([]).exit().remove();
        svg.selectAll(".rect").data([]).exit().remove();

        anchors = svg.selectAll(".dot")
            .data(createanchor1)
            .enter().append("circle")
            .attr("class", "dot")
            .attr("r", function (d) {
                return (d.r);
            })
            .attr("cx", function (d) {
                return (d.x);
            })
            .attr("cy", function (d) {
                return (d.y);
            })
            .style("fill", 'green');

        // Draw labels
        labels = svg.selectAll(".label")
            .data(labelsnodes)
            .enter()
            .append("text")
            .attr("class", "label")
            .attr('text-anchor', 'start')
            .text(function (d) {
                return d.name;
            })
            .attr("x", function (d) {
                return (d.x);
            })
            .attr("y", function (d) {
                return (d.y);
            })
            .attr("fill", "black")
            .on("click",d => tomasz1(d))
            

        // Size of each label
        var index = 0;
        labels.each(function () {
            labelsnodes[index].width = this.getBBox().width;
            labelsnodes[index].height = this.getBBox().height;
            index += 1;
        });

        // Draw data points
        circ = svg.selectAll(".circ")
            .data(labelsnodes)
            .enter().append("circle")
            .attr("class", ".circ")
            .attr("r", 20.0)
            .attr("cx", function (d) {
                return (d.x);
            })
            .attr("cy", function (d) {
                return (d.y - offset);
            })
            .style("fill", 'red')
            .attr('opacity', 0.0);

        // Draw links
        links = svg.selectAll(".link")
            .data(labelsnodes)
            .enter()
            .append("line")
            .attr("class", "link")
            .attr("x1", function (d) {
                return (d.x);
            })
            .attr("y1", function (d) {
                return (d.y);
            })
            .attr("x2", function (d) {
                return (d.x);
            })
            .attr("y2", function (d) {
                return (d.y);
            })
            .attr("stroke-width", 0.6)
            .attr("stroke", "gray");
        var sim_ann = d3.labeler()
            .label(labelsnodes)
            .anchor(createanchor1)
            .width(width)
            .height(height)
            .start(1000);
        }
}


function creategraph() {
// set the dimensions and margins of the graph


    d3.json("/home/data", function (json) {
        var labelPadding = 2;
        var nodes = json.network.nodes;
        var xcoord = [];
        var ycoord = []
        let nodescount = nodes.length;
        Json=json;
        
        nodes.forEach(function (nodes) {
            xcoord.push(parseFloat(nodes.coordinate.x));
            ycoord.push(parseFloat(nodes.coordinate.y));
        });
        var maxx = Math.max.apply(Math, xcoord);
        let minx = Math.min.apply(Math, xcoord);
        let maxy = Math.max.apply(Math, ycoord);
        let miny = Math.min.apply(Math, ycoord);

        var linearScaleX = d3.scaleLinear()
            .domain([minx, maxx])
            .range([0, 600]);
        var linearScaleY = d3.scaleLinear()
            .domain([miny, maxy])
            .range([0, 600]);
        // Initialize the links
       
        const link = svg
            .append("g")
            .attr("class", "line")
            .selectAll("g")
            .data(json.network.links)
            .enter().append("g")

        
        const line = link.append("line")
            .attr("x1", d => linearScaleX(d.firstNode.coordinate.x))
            .attr("y1", d => linearScaleY(d.firstNode.coordinate.y))
            .attr("x2", d => linearScaleX(d.secondNode.coordinate.x))
            .attr("y2", d => linearScaleY(d.secondNode.coordinate.y))
            .style("stroke", function (d) {
                if (d.linkType == 3) {
                    return "blue";
                } else if (d.linkType == 1) {
                    return "lightblue";
                }
            });
        const lableslink = link.append("text")
            .text(d => d.name)
            .attr("class","linklabel")
            .attr('x', d => linearScaleX((d.firstNode.coordinate.x + d.secondNode.coordinate.x) / 2) + 6)
            .attr('y', d => linearScaleY((d.firstNode.coordinate.y + d.secondNode.coordinate.y) / 2) + 6)
            .style("fill", "red")
            .on("click", d => tomasz(d));

        var node = svg.append("g")
            .attr("class", "nodes")
            .selectAll("g")
            .data(nodes)
            .enter().append("g");

        const circle = node.append("circle") //   .selectAll("circle")    
            .attr("cx", d => linearScaleX(d.coordinate.x))
            .attr("cy", d => linearScaleY(d.coordinate.y))
            .attr("r", 2)
            .style("fill", "#69b3a2");

        //var lables = node.append("text")
        //    .text(d => d.name)
        //    .attr('x', d => linearScaleX(d.coordinate.x) + 6)
        //    .attr('y', d => linearScaleY(d.coordinate.y) + 6)
        //    .on("click", d => tomasz1(d));
       
        createlabels(json.network);
       

        function tomasz(d) {
            $('#here_table').empty();
            var results = [];
            results = json.linksresults.filter(obj => obj.link.name === d.name);
            var m = results.length;

            var table = $('<table class="table table-striped"></table>');
            //var header = $('<th></th>');
            var headercol1 = $('<th><th>').text('flow');
            var headercol2 = $('<th><th>').text('velocity');
            table.append('<th><tr><td>TimeStep</td><td>frictionfactor</td><td>Flow</td><td>Velocity</td></tr></th>');
            //table.append(headercol2);
            //table.append(header);
            for (n = 0; n < m; n++) {
                var row = $('<tr><td>' + results[n].step.step + '</td><td>' + (Math.round(results[n].frictionfactor * 10000) / 10000).toFixed(4) + '</td><td>' + (Math.round(results[n].flow * 10000) / 10000).toFixed(4) + '</td><td>' + (Math.round(results[n].velocity * 10000) / 10000).toFixed(4) + '</td></tr>');
                table.append(row);
                //row.append(row1);
            }
            $('#here_table').append(table);
        }

        function tomasz1(d) {
            $('#here_table').empty();
            var results = [];
            results = json.nodesresults.filter(obj => obj.name === d.name);
            var m = results.length;
            var table = $('<table class="table table-striped"></table>');
            //var header = $('<th></th>');
            var headercol1 = $('<th><th>').text('flow');
            var headercol2 = $('<th><th>').text('velocity');
            table.append('<th><tr><td>TimeStep</td><td>Head</td><td>Demand</td><td>Pressure</td></tr></th>');
            //table.append(headercol2);
            //table.append(header);
            for (n = 0; n < m; n++) {
                var row = $('<tr><td>' + results[n].timestep.step + '</td><td>' + (Math.round(results[n].head * 10000) / 10000).toFixed(4) + '</td><td>' + (Math.round(results[n].demand * 10000) / 10000).toFixed(4) + '</td><td>' + (Math.round(results[n].pressure * 10000) / 10000).toFixed(4) + '</td></tr>');
                table.append(row);
                //row.append(row1);
            }
            $('#here_table').append(table);
        }

       // var lables = node.append("text")
       //     .text(d => d.name)
       //     .attr('x', d => linearScaleX(d.coordinate.x) + 6)
       //     .attr('y', d => linearScaleY(d.coordinate.y) + 6)
       //     .on("click", d => tomasz1(d));
    });
}
