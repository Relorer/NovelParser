const getContent = () => {
    return (
        "<div>" + window.__pg.map((i) => `<img src="${window.__info.img.url}/${i.u}"/>`).join("") + "</div>"
    );
};

return getContent();