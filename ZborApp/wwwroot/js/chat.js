class Message {
    constructor(idRazg, idUser, message, when) {
        this.idRazg = idRazg;
        this.message = message;
        this.when = when;
        this.idUser = idUser;
        this.slika = "";
        this.ime = "";
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
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

function sendMessage(idRazg, idUser) {
    
    //var user = document.getElementById("userInput").value;
    var message = document.getElementById("Poruka-" + idRazg).value;
    var messageObj = new Message(idRazg, idUser, message, new Date());
    connection.invoke("SendMessage", messageObj).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("Poruka-" + idRazg).value= "";
    event.preventDefault();
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

function pad(n) { return n < 10 ? '0' + n : n; }
