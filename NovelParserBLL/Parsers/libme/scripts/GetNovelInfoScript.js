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

const getInfo = async () => {
    const result = {
        Name:
            window.__DATA__.manga.engName ||
            window.__DATA__.manga.rusName ||
            window.__DATA__.manga.slug,
        Author:
            [
                ...[...document.querySelectorAll(".media-info-list__item")].find(
                    (item) => item.children[0].innerText === "Автор"
                )?.children[1].children ?? [],
            ]
                .map((ch) => ch.textContent.trim())
                .join(", ") || "No Author",
        Description: document
            .querySelector(".media-description__text")
            ?.textContent.trim(),
    };

    if (arguments[0] === true) {
        const coverURL = (
            (window.__DATA__.manga.name.indexOf("'") < 0 &&
                document.querySelector(`img[alt='${window.__DATA__.manga.name}']`)) ||
            document.querySelector("img.media-header__cover") ||
            document.querySelector("div.media-sidebar__cover.paper > img")
        )?.src;

        if (coverURL) {
            await forceDownload(coverURL);
            await new Promise((r) => setTimeout(r, 2000));
        }

        result.Cover = {
            URL: coverURL,
        };
    }

    return JSON.stringify(result);
};

return await getInfo();