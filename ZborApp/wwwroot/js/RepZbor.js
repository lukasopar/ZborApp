function objavi(id) {
    var privatno = "#Privatno-" + id;
    var javno = "#Javno-" + id;
    //pošalji ajax
    data = { value: id + '' };
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Repozitorij/ObjaviZbor',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    });
    $(privatno).hide();
    $(javno).show();

}

function privatiziraj(id) {
    var privatno = "#Privatno-" + id;
    var javno = "#Javno-" + id;
    //pošalji ajax
    data = { value: id + '' };
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Repozitorij/PrivatizirajZbor',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        }
    });
    $(javno).hide();
    $(privatno).show();

}


function bindEdit() {
    $('.edit').bind('click', function () {
        let id = $(this).attr('data-id');
        let edit = "#Naziv-" + id;
        $(edit).attr('contentEditable', true);
        $(edit).focus();
    });
    $('.name').blur(
        function () {
            console.log($(this).text());
            let id = $(this).attr('data-id');
            //Zove se ajax i mijenja se u sazetku.
            data = { id: id + '', tekst: $(this).text() };

            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                url: '/Repozitorij/PromjenaNazivaZbor',
                data: JSON.stringify(data),
                xhrFields: {
                    withCredentials: true
                }

            });
            $(this).attr('contentEditable', false);
        });
}

function getDialog(id) {
    $("#textdialog").val(id);
        $( function() {
            $("#dialog").dialog();
      } );
  
}