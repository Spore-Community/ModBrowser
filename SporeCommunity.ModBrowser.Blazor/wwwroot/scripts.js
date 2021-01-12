window.getUserAgent = () => {
    return navigator.userAgent;
};

setTimeout(function () {
    let appElement = document.getElementById("app");
    if (appElement.firstElementChild.nodeName !== "ARTICLE") {
        let node = document.createElement("p");
        let content = document.createTextNode("Not loading? Your browser might not be supported.");
        node.appendChild(content);
        appElement.appendChild(node);

        node = document.createElement("a");
        node.setAttribute("href", "https://discord.gg/YUcJWtb")
        content = document.createTextNode("Go to Discord for help.");
        node.appendChild(content);
        appElement.appendChild(node);

        node = document.createElement("p");
        content = document.createTextNode("User Agent: " + getUserAgent());
        node.appendChild(content);
        appElement.appendChild(node);
    }
}, 5000);