function prihvati(id) {
    data = { Value: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/PrihvatiPrijavu',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        console.log(response)
        prijava = '#Prijava-' + id;

        $(prijava).remove();
        if ($("#listaPrijava").children().length === 1) $("#nemaPrijava").show();
        newDiv = '<div style="width:100%; padding-top:7px" class="d-flex flex-row align-items-center justify-content-between"><a href="#">' + response.imeIPrezime + '</a><div class="dropdown no-arrow"><a class="dropdown-toggle" href="#" role="button" id="dropdownMenuLink" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"> <i class="fa fa-ellipsis-v"></i> </a><div class="dropdown-menu dropdown-menu-right shadow animated--fade-in" aria-labelledby="dropdownMenuLink"><div class="dropdown-header">Mogućnosti</div><a class="dropdown-item" href="#">Promjena glasa</a><form action="/Zbor/ObrisiClana" method="post"><input id="IdBrisanje" name="IdBrisanje" value="' + response.id + '" hidden style="display:none"/><button class="dropdown-item" type="submit">Izbaci</button></form></div></div></div>';
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
        url: '/Zbor/OdbijPrijavu',
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

function odbaci(id) {
    data = { Value: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/ObrisiPoziv',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        poziv = '#Poziv-' + id;
        console.log(poziv);
        $(poziv).remove();
        if ($("#listaPoziva").children().length === 1) $("#nemaAktivnihPoziva").show();

    })
        .always(function () {
            g = 2;
        });

}
function moderiraj(id, idZbor) {
   

    data = { IdKorisnik: id + '', idCilj: idZbor + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/NoviModerator',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        $("#nemaModeratora").hide();
        newDiv = '<div id="Mod-' + response.id + '" style="width:100%; padding-bottom:7px" class="d-flex flex-row align-items-center justify-content-between"><div> <a href="#">' + response.imeIPrezime + '</a> </div> <div>    <span class="hoverable"><i onclick="odmoderiraj(\'' + response.id + '\')" class="fa fa-times"></i></span></div></div>';
        $("#listaModeratora").append(newDiv);




    })
        .always(function () {

            g = 2;
        });
}
function odmoderiraj(id) {
    data = { Value: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/ObrisiModeratora',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        mod = '#Mod-' + id;
        $(mod).remove();
        if ($("#listaModeratora").children().length === 1) $("#nemaModeratora").show();

    })
        .always(function () {
            g = 2;
        });

}

function pretraga(ele, id) {
    if (event.keyCode != 13) {
        return;
    }
    inputPolje = '#pret';
    $(inputPolje).prop('disabled', true)
    data = { Tekst: ele.value + '', Id: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/PretragaKorisnika',
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
            var $divcol1 = $('<div class="col-md-3"></div>');
            var $link = $('<a href="#">' + response[i]['imeIPrezime'] + '</a>');
            $divcol1.append($link);
            $newdiv.append($divcol1);


            var $divcol2 = $('<div class="col-md-3" id="B-' + response[i].id + '" ></div>');
            var $button = ('<button type="button"  class="btn btn-outline-secondary btn-sm" data-toggle="modal" data-target="#exampleModal" data-ime="' + response[i].imeIPrezime + '" data-id="' + response[i].id + '">Pozovi</button>');
            $divcol2.append($button);
            $newdiv.append($divcol2);

            var $divcol3 = $('<div class="col-md-3" id="T-' + response[i].id + '" style="display: none"></div>');
            var $tekst = $('<text   class="text-success">Poziv poslan.</text>');
            $divcol3.append($tekst);
            $newdiv.append($divcol3);


            $("#rezultati").append($newdiv);

        }

    })
        .always(function () {

            $(inputPolje).prop('disabled', false)
        });

}
function prijavaAjax() {
    modal = $('#exampleModal')
    data = { Id: modal.find('#id').text() + '', Naziv: '@Model.Zbor.Id.ToString()', Poruka: modal.find('#poruka').val() + '' }
    console.log(data)
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '@Url.Action("PozivZaZbor", "Zbor")',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        modal.modal('toggle')
        id = modal.find('#id').text() + ''
        testB = '#B-' + id
        testC = '#T-' + id
        $(testB).hide();
        $(testC).show("slow");
        console.log(response);
        var newDiv = '<div id="Poziv-' + response.id + '" style="width:100%; padding-bottom:7px" class="d-flex flex-row align-items-center justify-content-between"><div> <a href="#" data-toggle="tooltip" data-placement="left" title="' + data["Poruka"] + '">' + response.imeIPrezime + '</a> ' + response.datum + '</div><div><span class="hoverable"><i onclick="odbaci(\'' + response.id + '\')" class="fa fa-times"></i></span></div></div>';
        console.log(newDiv);
        $("#nemaAktivnihPoziva").hide();

        $("#listaPoziva").append(newDiv);

        $('[data-toggle="tooltip"]').tooltip();
    });





}