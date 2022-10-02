const forceDownload = async (url) => {
    const fileName = url.substring(url.lastIndexOf("/") + 1);
    await new Promise((resolve) => {
        const img = document.createElement("img");
        img.onload = () => {
            var tag = document.createElement("a");
            tag.href = url;
            tag.download = fileName;
            document.body.appendChild(tag);
            tag.click();
            document.body.removeChild(tag);
            resolve(undefined);
        };
        img.onerror = () => {
            resolve(undefined);
        };

        img.src = url;
    });
};

const getContent = async () => {
    const content = document.querySelector(".reader-container");

    for (let img of content.querySelectorAll("img") || []) {
        console.log(arguments)
        if (arguments[0] === true) {
            const url = img.getAttribute("data-src");
            img.setAttribute("data-src", null);
            await forceDownload(url);
            await new Promise((r) => setTimeout(r, 200));
            img.src = url;
        } else {
            img.remove();
        }
    }

    if (arguments[0] === true) {
        await new Promise((r) => setTimeout(r, 1000));
    }

    return content?.innerHTML;
};

return await getContent();