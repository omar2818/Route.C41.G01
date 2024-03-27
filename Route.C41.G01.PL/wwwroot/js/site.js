// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var searching = document.getElementById("searching");

searching.addEventListener("keyup", function () {

    let xhr = new XMLHttpRequest();

    let url = `https://localhost:44359/Employee/Index?searching=${searching.value}`;
    xhr.open("Post", url, true);

    xhr.onreadystatechange = function () {
        if (this.readyState == 4 $$ this.status == 202){
    console.log(this.Response);
        }
    }

xhr.send();
})

