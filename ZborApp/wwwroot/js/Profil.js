function lajkObavijesti(id, idKorisnik) {
    gumbObavijest = '#BtnO-' + id;
    brojLajkovaObavijest = '#LajkO-' + id;
    data = { IdCilj: id + '', IdKorisnik: idKorisnik+'' };
    if ($(gumbObavijest).hasClass("btn-outline-secondary")) {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            url: '/Zbor/lajKObavijesti',
            data: JSON.stringify(data),
            xhrFields: {
                withCredentials: true
            }
        });
      
    }
    else {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            url: '/Zbor/UnlajkObavijesti',
            data: JSON.stringify(data),
            xhrFields: {
                withCredentials: true
            }
        });
       
    }


}

function lajkKomentara(id, idKorisnik) {
    gumbKomentar = '#BtnK-' + id;
    brojLajkovaKomentar = '#LajkK-' + id;

    data = { IdCilj: id + '', IdKorisnik: idKorisnik+'' };
    if ($(gumbKomentar).hasClass("font-weight-bold")) {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            url: '/Zbor/UnlajkKomentara',
            data: JSON.stringify(data),
            xhrFields: {
                withCredentials: true
            }
        });

    }
    else {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            url: '/Zbor/LajkKomentara',
            data: JSON.stringify(data),
            xhrFields: {
                withCredentials: true
            }
        });
      
    }
}
function obrisi(id) {
    data = { Value: id + '' };
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/ObrisiKomentar',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {
        

    })
        .always(function () {
            g = 2;
        });

}
function test(ele, id) {
    if (event.keyCode !== 13) {
        return;
    }
    inputPolje = '#noviKomentar-' + id
    $(inputPolje).prop('disabled', true)
    data = { Tekst: ele.value + '', IdObavijest: id + '' }
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/NoviKomentar',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    }).done(function (response) {


        $(inputPolje).val('');




    })
        .always(function () {

            $(inputPolje).prop('disabled', false)
        });

}