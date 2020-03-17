function promjenaRazgovora(id) {
    novePoruke = '#Poruke-' + id;
    noviRazg = '#Razg-' + id;

    $("#poruke").children().hide();
    $("#lista").children().removeClass("active_chat");

    $(novePoruke).show();
    $(noviRazg).addClass("active_chat");

}