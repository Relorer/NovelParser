const getContent = () => {
    return (
        "<div>" + window.__pg.map((i) => `<img src="/manga/${__DATA__.manga.slug}/chapters/${window.__info.current.id}/${i.u}"/>`).join("") + "</div>"
    );
};

return getContent();