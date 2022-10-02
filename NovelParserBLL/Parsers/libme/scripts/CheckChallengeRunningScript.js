if (document.querySelector(".auth-form a") !== null) {
    document.querySelector(".auth-form a").click();
    return "true";
}
else return JSON.stringify(document.querySelector("#challenge-running") !== null);