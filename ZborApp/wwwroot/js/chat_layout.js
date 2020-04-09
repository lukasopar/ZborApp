function renderChat() {
    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Poruke/DohvatiPoruke',
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        var i = 0;
        for (i = 0; i < 3; i++) {
            var porukaDiv = '<a id="Header-' + response.poruke[i].id + '" class="dropdown-item d-flex align-items-center" href="/Poruke/Poruka/' + response.poruke[i].id + '"><div class="dropdown-list-image mr-3"><img class="rounded-circle" src="' + response.poruke[i].slika + '" alt=""></div><div id="Procitano-' + response.poruke[i].id + '"><div class="text-truncate">' + response.poruke[i].poruka + '</div><div class="small text-gray-500">' + response.poruke[i].naziv + ' ' + response.poruke[i].datum + '</div></div></a>'
            $("#poruke_layout").append(porukaDiv);
            if (response.poruke[i].procitano === false) {
                var proc = "#Procitano-" + response.poruke[i].id;
                $(proc).addClass("font-weight-bold");
            }
            if (response.neprocitane > 0) {
                $("#messageCounter").text(response.neprocitane);
            }
        }
    })
        .always(function () {
            g = 2;
        });

}


var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start();

//Disable send button until connection is established
//document.getElementById("sendButton").disabled = true;

connection.on("ChangeHeader", function (response) {
    var porukaDiv = '<a id="Header-' + response.id + '" class="dropdown-item d-flex align-items-center" href="/Poruke/Poruka/' + response.id + '"><div class="dropdown-list-image mr-3"><img class="rounded-circle" src="' + response.slika + '" alt=""></div><div id="Procitano-' + response.id + '"><div class="text-truncate">' + response.poruka + '</div><div class="small text-gray-500">' + response.naziv + ' ' + response.datum + '</div></div></a>'
    var head = "#Header-" + response.id;
    $(head).remove();
    $("#poruke_layout").prepend(porukaDiv);

   
    if ($("#poruke_layout").children().length > 3) {
        $("#poruke_layout").children().last().remove();
    }
    if (response.procitano === false) {
        var proc = "#Procitano-" + response.id;
        $(proc).addClass("font-weight-bold");
    }
    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Poruke/DohvatiNeprocitano',
        xhrFields: {
            withCredentials: true
        }
    }).done(function (broj) {
        if (broj > 0) {
            $("#messageCounter").text(broj);
        }
        else if (broj === 0) {
            $("#messageCounter").text("");

        }
    })
        .always(function () {
            g = 2;
        });
});

connection.on("LajkObavijesti", function (response) {
    brojLajkovaObavijest = '#LajkO-' + response.id;
    gumbObavijest = '#BtnO-' + response.id;
    if (response.lajk === true) {
        $(brojLajkovaObavijest).text(response.brojLajkova);
        if (response.jesamja === true) {
            $(gumbObavijest).removeClass("btn-outline-secondary");
            $(gumbObavijest).addClass("btn-secondary");
        }
    }
    else {
        $(brojLajkovaObavijest).text(response.brojLajkova);
        if (response.jesamja === true) {
            $(gumbObavijest).removeClass("btn-secondary");
            $(gumbObavijest).addClass("btn-outline-secondary");
        }
    }
    
});

connection.on("LajkKomentara", function (response) {
    brojLajkovaObavijest = '#LajkK-' + response.id;
    gumbObavijest = '#BtnK-' + response.id;
    if (response.lajk === true) {
        $(brojLajkovaObavijest).text(response.brojLajkova);
        if (response.jesamja === true) {
            $(gumbKomentar).addClass("font-weight-bold");

        }
    }
    else {
        $(brojLajkovaObavijest).text(response.brojLajkova);
        if (response.jesamja === true) {
            $(gumbKomentar).removeClass("font-weight-bold");

        }
    }

});

connection.on("ObrisiKomentar", function (response) {
    kom = '#Kom-' + response.id;
    $(kom).remove();

});

connection.on("NoviKomentar", function (response) {
    console.log(response)
    idDiv = "#Komentari-" + response.idObavijest;
    var $listItem = $('<li class="list-group-item " style="margin-bottom:0px; padding-bottom:0px"  id="Kom-' + response.id + '"></li>')
    $(idDiv).append($listItem);

    var $kom = $('<div class="col-md-6 rounded-pill" style="display: inline-block; background-color:gainsboro;padding-bottom:10px; padding-left:10px; padding-top:10px"></div>');
    var $d = $('<div class="d-flex justify-content-between align-items-center"></div>');
    var $div1 = $('<div><img src="' + response.slika + '" alt="profpic" style="border-radius:50%; width:30px; height:30px"><span style="color:cornflowerblue">' + response.imeIPrezime + '</span>: ' + response.tekst + '</div>');
    $($d).append($div1);

    var $div2 = $('<div><a class="hoverable" onclick="obrisi(\'' + response.id + '\')"><i class="fa fa-trash-o"></i></a>  </div>');
    $($d).append($div2);
    $($kom).append($d);

    $($listItem).append($kom);

    var $upperdiv = $('<div style="padding-top:0px" class="col-md-6"></div>')
    var $flex = $('<div class="d-flex justify-content-between align-items-center" style="font-size:15px"></div>')
    var $newdiv2 = $('<div>' + response.datum + '</div>')

    var $last = $('<div class="row"><div class="hoverable" data-id="' + response.id + '" data-name="komentar" data-toggle="modal" data-target="#statistika">  <i class="fa fa-thumbs-o-up"></i> <span class="badge badge-secondary badge-counter" id="LajkK-' + response.id + '">0</span> </div><a class="hoverable" style="color:blue" id="BtnK-' + response.id + '" onclick="lajkKomentara(\'' + response.id + '\', \'' + response.idkorisnik + '\')">Sviđa mi se</a> </p></div>');
    $($flex).append($newdiv2);
    $($flex).append($last);
    $($upperdiv).append($flex);

    $($listItem).append($upperdiv);

    $(idDiv).append($listItem);

});
