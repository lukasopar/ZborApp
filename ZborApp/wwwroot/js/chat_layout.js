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
    if (response.procitano == false) {
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

