return (() => {
    const dict = {};
    (window.__DATA__.chapters.branches.length > 0
        ? window.__DATA__.chapters.branches
        : [{ id: "nobranches", name: "none" }]
    ).forEach(
        (br) =>
        (dict[br.name] = (() => {
            const chapter = {};
            window.__DATA__.chapters.list
                .filter((ch) => ch.branch_id === br.id || br.id === "nobranches")
                .forEach(
                    (ch) =>
                    (chapter[ch.index] = {
                        Name: ch.chapter_name,
                        Url: `https://ranobelib.me/${window.__DATA__.manga.slug}/v${ch.chapter_volume}/c${ch.chapter_number}`,
                        Number: ch.chapter_number,
                    })
                );
            return chapter;
        })())
    );
    return JSON.stringify(dict);
})();