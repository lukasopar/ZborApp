// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$('#napustanjeZbora').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget) // Button that triggered the modal
    var id = button.data('id')
    var modal = $(this)
    modal.find('#idNapusti').text(id);

})
function napustiZbor() {
    id = $("#idNapusti").text() + '';
    data = { Value: id + '' };
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        url: '/Zbor/NapustiZbor',
        data: JSON.stringify(data),
        xhrFields: {
            withCredentials: true
        },
        statusCode: {
            409: function () {
                alert("Vi ste voditelj zbora. Potrebno je prije napuštanja odabrati novog voditelja.");
            }
        }
    }).done(function (response) {

        window.location.href = "/Home/Index";

    })
        .always(function () {
        });
}
