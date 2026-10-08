let registrationUrl = "https://localhost:7288/registration";
let loginUrl = "https://localhost:7288/login";
let userUrl = "https://localhost:7288/getUserData";

function get_data(){
    let token = localStorage.getItem("jwtToken");

    if(token != null){
        fetch(userUrl, {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + token
            }
        }).then(response =>{
            return response.json();
        }).then(response =>{
            document.getElementById("name").textContent = response.name;
            document.getElementById("email").textContent = response.email;
        });
    }
    else{
        console.log("не авторизован");
    }
}
function registration(event){
    event.preventDefault();


    let email = document.getElementById("registration-email").value;
    let password = document.getElementById("registration-password").value;
    let name = document.getElementById("registration-name").value;

    fetch(registrationUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({name: name, email: email, password: password})
    }).then(response => {
        if(response.ok){
            console.log("Аккаунт создан...");
        }

        document.getElementById("registration-email").value = "";
        document.getElementById("registration-password").value = "";
        document.getElementById("registration-name").value = "";
    });
}

function login(event){
    event.preventDefault();

    let email = document.getElementById("login-email").value;
    let password = document.getElementById("login-password").value;

    fetch(loginUrl, {
        method: "POST",
        headers:{
            "Content-Type": "application/json"
        },
        body: JSON.stringify({email: email, password: password}) 
    })
    .then(response => {
        if(response.ok){
            document.getElementById("login-email").value = "";
            document.getElementById("login-password").value = ""; 
        }
        return response.text()
    })
    .then(response =>{
        localStorage.setItem("jwtToken", response);
        location.reload();
    });
}

function logOut(event){
    localStorage.removeItem("jwtToken");
    location.reload();
}

get_data();