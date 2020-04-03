class Message {
    constructor(idRazg, idUser, message, when, kontakti) {
        this.idRazg = idRazg;
        this.message = message;
        this.when = when;
        this.idUser = idUser;
        this.slika = "";
        this.ime = "";
        this.kontakti = kontakti;
        this.imeRazgovora = "";
        this.popis = "";
    }
}


var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable send button until connection is established
//document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (userId, message) {
    console.log(formatDate(message.when));

    if (userId === message.idUser) {
        //moja poruka
        let newDiv = '<div class="outgoing_msg"> <div class="sent_msg"><p>' + message.message + ' </p><span class="time_date">' + formatDate(message.when) + '    |    ' + formatTime(message.when) + '</span ></div></div>';
        let chat = "#Chat-" + message.idRazg;
        let sazetak = "#Sazetak-" + message.idRazg;
        $(sazetak).html("Ti: " + message.message);

        $(chat).append(newDiv);
    }
    else {
        //dobivena poruka
        let newDiv = '<div class="incoming_msg"> <div class="incoming_msg_img"> <img src="' + message.slika + '" alt="sunil"> </div><div class="received_msg"><div class="received_withd_msg"><span class="time_date">' + message.ime + '</span><p>' + message.message + ' </p><span class="time_date">' + formatDate(message.when) + '    |    ' + formatTime(message.when) + '</span ></div></div></div>';   
        let chat = "#Chat-" + message.idRazg;
        let sazetak = "#Sazetak-" + message.idRazg;
        $(sazetak).html(message.message);

        $(chat).append(newDiv);
    }
    let dat = "#DatZadnje-" + message.idRazg;
    $(dat).html(formatDate(message.when) + " " + formatTime(message.when));
    setOnTop(message.idRazg);
    if (userId != message.idUser) {
        var novo = "#Novo-" + message.idRazg;
        $(novo).show();
        
    }
    let chat = "#Chat-" + message.idRazg;
    $(chat).scrollTop($(chat)[0].scrollHeight - $(chat)[0].clientHeight);
});

connection.on("ReceiveNewConversation", function (userId, message) {
    //sazetak desno
    var newDiv = "";
    if (message.popis.split(",").length === 1) {
        newDiv = '<div class="chat_list" id="Razg-' + message.idRazg + '" onclick="promjenaRazgovora(\'' + message.idRazg + '\')"><div class="chat_people" ><div class="chat_img"><img src="' + message.slika + '" alt="profpic"></div> <div class="chat_ib"><h5><b>' + message.popis + '</b> <span class="chat_date" id="DatZadnje-' + message.idRazg + '">' + formatDate(message.when) + ' ' + formatTime(message.when) + '</span></h5> <p><span  id="Sazetak-' + message.idRazg + '">' + getSazetak(userId, message) + '</span><span class="novo" id="Novo-' + message.idRazg + '" style="display:none">Novo!</span></p></div></div ></div>';
    }
    else { 
        newDiv = '<div class="chat_list" id="Razg-' + message.idRazg + '" onclick="promjenaRazgovora(\'' + message.idRazg + '\')"><div class="chat_people" ><div class="chat_img"><img src="' + message.slika + '" alt="profpic"></div> <div class="chat_ib"><h5><b id="Naslov-' + message.idRazg + '">' +"Grupa" + '</b> <span class="chat_date" id="DatZadnje-' + message.idRazg + '">' + formatDate(message.when) + ' ' + formatTime(message.when) + '</span></h5> <p><span  id="Sazetak-' + message.idRazg + '">' + getSazetak(userId, message) + '</span><span class="novo" id="Novo-' + message.idRazg + '" style="display:none">Novo!</span></p></div></div ></div>';
    }
    $("#lista").prepend(newDiv);
    // poruke lijevo
    var divPoruka = "";
    if (message.popis.split(",").length === 1) {
        divPoruka = '<div id="Poruke-' + message.idRazg + '" style="display:none" onclick="procitano(\'' + message.idRazg + '\')"><div class="headind_name" ><p><span class="editable" data-id="' + message.idRazg + '">Razgovor </span><span>(' + message.popis + ')</span></p></div><div class="msg_history" id="Chat-' + message.idRazg + '">' + getPoruka(userId, message) + '</div><div class="type_msg"><div class="input_msg_write" ><textarea id="Poruka-' + message.idRazg + '" type="text" class="write_msg form-control" style="resize:vertical" onkeydown="enter(\'' + message.idRazg + '\')" placeholder="Unesi poruku" /><button id="Gumb-' + message.idRazg + '" class="msg_send_btn" type="button" onclick="sendMessage(\'' + message.idRazg + '\', \'' + userId + '\')"><i class="fa fa-paper-plane-o" aria-hidden="true"></i></button></div ></div ></div>';
    }
    else {
        divPoruka = '<div id="Poruke-' + message.idRazg + '" style="display:none" onclick="procitano(\'' + message.idRazg + '\')"><div class="headind_name" ><p><span class="editable" data-id="' + message.idRazg + '">Grupa </span><span><i class="fa fa-user hoverable" data-toggle="tooltip" data-title="' + message.popis + '"></i>   </span></p></div><div class="msg_history" id="Chat-' + message.idRazg + '">' + getPoruka(userId, message) + '</div><div class="type_msg"><div class="input_msg_write" ><textarea id="Poruka-' + message.idRazg + '" type="text" class="write_msg form-control" style="resize:vertical" onkeydown="enter(\'' + message.idRazg + '\')" placeholder="Unesi poruku" /><button id="Gumb-' + message.idRazg + '" class="msg_send_btn" type="button" onclick="sendMessage(\'' + message.idRazg + '\', \'' + userId + '\')"><i class="fa fa-paper-plane-o" aria-hidden="true"></i></button></div ></div ></div>';

    }
    $("#poruke").append(divPoruka);
    bindEdit();
    $('[data-toggle="tooltip"]').tooltip();

    if (userId !== message.idUser) { 
        var novo = "#Novo-" + message.idRazg;
        $(novo).show();
        var proc = "#Procitano-" + message.idRazg;
        $(proc).addClass("font-weight-bold");
    }
    let chat = "#Chat-" + message.idRazg;
    $(chat).scrollTop($(chat)[0].scrollHeight - $(chat)[0].clientHeight);
});



connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

function sendMessage(idRazg, idUser) {
    
    //var user = document.getElementById("userInput").value;
    var message = document.getElementById("Poruka-" + idRazg).value;
    if (message.trim() === "") { return; }
    var messageObj = new Message(idRazg, idUser, message, getDatumZaASP(), []);
    connection.invoke("SendMessage", messageObj).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("Poruka-" + idRazg).value= "";
    event.preventDefault();
}

function newConversation(idUser) {

    //var user = document.getElementById("userInput").value;
    var message = document.getElementById("novaPoruka").value;

    var vals = $('#basic').attr("data-id").split(',');
    if (vals.length === 1 && vals[0] === "") {
        alert("Unesite primatelja!");
        return;
    }
    if (message.trim() === "") {
        alert("Unesite poruku!");
        return;    
    }
    var messageObj = new Message(null, idUser, message, getDatumZaASP(), vals);
    connection.invoke("NewConversation", messageObj)
    $("#inputDiv").empty();
    inputField = ' <input class="form-control" style="height:50px; width:200px" id="basic" placeholder="Pretraži.." />';
    $("#inputDiv").append(inputField);
    $('#basic').magicsearch({
        dataSource: dataSource,
        fields: ['firstName', 'lastName'],
        id: 'id',
        format: '%firstName% %lastName%',
        multiple: true,
        multiField: 'firstName',
        multiStyle: {
            space: 5,
            width: 80
        }
    });

    document.getElementById("novaPoruka").value = "";
    $("#exampleModal").modal('toggle');

}

function formatDate(dateString) {
    var date = new Date(dateString);
    var newDate = pad(date.getDate()) + "." + pad((date.getMonth() + 1)) + "." + pad(date.getFullYear());
    return newDate;
}

function formatTime(dateString) {
    var date = new Date(dateString);
    newDate = pad(date.getHours()) + ":" + pad(date.getMinutes());
    return newDate;
}

function setOnTop(id) {
    let razg = "#Razg-" + id;
    $("#lista").prepend($(razg));
}

function getSazetak(idUser, message) {
    if (idUser === message.idUser) {
        //moja poruka
        return "Ti: " + message.message;
    }
    else return message.message;
}

function getPoruka(userId, message) {
    if (userId === message.idUser) {
        //moja poruka
        let newDiv = '<div class="outgoing_msg"> <div class="sent_msg"><p>' + message.message + ' </p><span class="time_date">' + formatDate(message.when) + '    |    ' + formatTime(message.when) + '</span ></div></div>';
        return newDiv;
    }
    else {
        //dobivena poruka
        let newDiv = '<div class="incoming_msg"> <div class="incoming_msg_img"> <img src="' + message.slika + '" alt="sunil"> </div><div class="received_msg"><div class="received_withd_msg"><span class="time_date">' + message.ime + '</span><p>' + message.message + ' </p><span class="time_date">' + formatDate(message.when) + '    |    ' + formatTime(message.when) + '</span ></div></div></div>';
        return newDiv;
    }
}

function getDatumZaASP() {
    var date = new Date();
    var day = pad(date.getDate());       // yields date
    var month = pad(date.getMonth() + 1);    // yields month (add one as '.getMonth()' is zero indexed)
    var year = date.getFullYear();  // yields year
    var hour = pad(date.getHours());     // yields hours 
    var minute = pad(date.getMinutes()); // yields minutes
    var second = pad(date.getSeconds()); // yields seconds

    // After this construct a string with the above results as below
    var time = day + "/" + month + "/" + year + " " + hour + ':' + minute + ':' + second; 
    return time;
}

function procitano(id) {
    var element = "#Novo-" + id;
    if ($(element).is(":visible")) {
        //pošalji ajax
        data = { value: id + '' };
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            url: 'Poruke/Procitano',
            data: JSON.stringify(data),
            xhrFields: {
                withCredentials: true
            }
        }).done(function () {
            let razg = "#Procitano-" + id;
            $(razg).removeClass("font-weight-bold");
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
        
        $(element).hide();

    }

    //pošalji ajax

}


function pad(n) { return n < 10 ? '0' + n : n; }
