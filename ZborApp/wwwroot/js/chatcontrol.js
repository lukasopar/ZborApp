function promjenaRazgovora(id) {
    novePoruke = '#Poruke-' + id;
    noviRazg = '#Razg-' + id;

    $("#poruke").children().hide();
    $("#lista").children().removeClass("active_chat");

    $(novePoruke).show();
    $(noviRazg).addClass("active_chat");
    let chat = "#Chat-" + id;
    $(chat).scrollTop($(chat)[0].scrollHeight - $(chat)[0].clientHeight);
    procitano(id);
}