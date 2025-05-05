local Vortex = {}

Vortex.name = "Aqua/Vortex"
Vortex.canBackground = true
Vortex.canForeground = true

Vortex.fieldInformation = {
    color1 = {
        fieldType = "color",
        useAlpha = false,
    },
    color2 = {
        fieldType = "color",
        useAlpha = false,
    },
}

Vortex.defaultData = {
    only = "*",
    exclude = "",
    tag = "",
    flag = "",
    notflag = "",
    color1 = "c42222",
    color2 = "2222c4",
    duration = 4.0,
}

Vortex.fieldOrder = {
    "only", "exclude", "tag", "flag", "notflag",
    "color1", "color2", "duration",
}

return Vortex