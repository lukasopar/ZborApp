function prihvati(id) {
    data = { Value: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/PrihvatiPrijavuProjekt',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        prijava = '#Prijava-' + id;

        $(prijava).remove();
        if ($("#listaPrijava").children().length === 1) $("#nemaPrijava").show();
        newDiv = '<div id="Clan-' + response.id + '" style="width:100%; padding-top:7px" class="d-flex flex-row align-items-center justify-content-between"><a  class="hoverable" data-id="' + response.idkorisnik + '" data-toggle="modal" data-target="#statistika">' + response.imeIPrezime + '</a><div class="dropdown no-arrow"><a class="dropdown-toggle" href="#" role="button" id="dropdownMenuLink" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"> <i class="fa fa-ellipsis-v"></i> </a><div class="dropdown-menu dropdown-menu-right shadow animated--fade-in" aria-labelledby="dropdownMenuLink"><div class="dropdown-header">Mogućnosti</div><button id="Promjena-' + response.id + '" class="dropdown-item" data-toggle="modal" data-target="#promjenaGlasa" data-ime="' + response.imeIPrezime + '" data-id="' + response.id + '" data-glas="1">Promjena glasa</button><form action="/Zbor/ObrisiClanaProjekta" method="post"><input id="IdBrisanje" name="IdBrisanje" value="' + response.id + '" hidden style="display:none" type="text" data-val="true"/><button class="dropdown-item" type="submit">Ukloni s projekta</button></form></div></div></div>';
        $("#Nerazvrstani").append(newDiv);


    })
        .always(function () {
            g = 2;
        });

}

function odbij(id) {
    data = { Value: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/OdbijPrijavuProjekt',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        prijava = '#Prijava-' + id

        $(prijava).remove();
        if ($("#listaPrijava").children().length === 1) $("#nemaPrijava").show();

    })
        .always(function () {
            g = 2;
        });

}

function pretraga(ele, id) {
    if (event.keyCode !== 13) {
        return;
    }
    inputPolje = '#pret';
    $(inputPolje).prop('disabled', true)
    data = { Tekst: ele.value + '', Id: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/PretragaKorisnikaProjekt',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        console.log(response)
        $("#rezultati").empty();
        var i;
        for (i = 0; i < response.length; i++) {
            var $newdiv = $('<div class="row d-flex flex-row align-items-center justify-content-between" style="padding-bottom:3px; padding-top:7px"></div>');
            var $divcol1 = $('<div class="col-md-5"></div>');
            var $link = $('<a href="#">' + response[i].imeIPrezime + '</a>');
            $divcol1.append($link);
            $newdiv.append($divcol1);


            var $divcol2 = $('<div class="col-md-3" id="B-' + response[i].id + '" ></div>');
            var $button = ('<button type="button"  class="btn btn-sm btn-outline-secondary" onclick="dodajClan(\'' + response[i].id + '\')">Dodaj</button>');
            $divcol2.append($button);
            $newdiv.append($divcol2);
            var g = 22;
            var $divcol3 = $('<div class="col-md-3" id="T-' + response[i].id + '" style="display: none"></div>');
            var $tekst = $('<text   class="text-success">Dodan.</text>');
            $divcol3.append($tekst);
            $newdiv.append($divcol3);


            $("#rezultati").append($newdiv);

        }

    })
        .always(function () {

            $(inputPolje).prop('disabled', false)
        });

}

function promjenaGlasa() {
    modal = $('#promjenaGlasa');
    data = { Id: modal.find('#id').text() + '', Poruka: modal.find('#selectGlasa').val() + '' };
    console.log(data);
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/PromjenaUloge',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        modal.modal('toggle');
        id = modal.find('#id').text() + '';
        clan = '#Clan-' + id;
        promjena = '#Promjena-' + id;

        idGlas = "." + data.Poruka;
        $(clan).detach().appendTo(idGlas);
        $(promjena).attr("data-glas", data.Poruka);
        $(promjena).data("glas", data.Poruka);


    }).always(function () {

        g = 2
    });
}
