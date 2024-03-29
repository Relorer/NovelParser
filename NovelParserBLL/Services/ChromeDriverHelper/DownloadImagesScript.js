﻿const forceDownload = async (img) => {
    const url = img.substring(0, img.lastIndexOf("?name="));
    const fileName = img.substring(img.lastIndexOf("?name=") + 6);
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

const downloadImages = async (imgs) => {
    for (const img of imgs) {
        await forceDownload(img);
        await new Promise((r) => setTimeout(r, 200));
    }
    await new Promise((r) => setTimeout(r, 1000));
};

return await downloadImages(arguments);