local AndromedaField = {}

AndromedaField.name = "Aqua/Andromeda Field"
AndromedaField.canBackground = true
AndromedaField.canForeground = true

AndromedaField.fieldInformation = {
    base_color = {
        fieldType = "color",
        useAlpha = false,
    },
    offset_color = {
        fieldType = "color",
        useAlpha = false,
    },
    layer_count = {
        fieldType = "integer",
    },
}

AndromedaField.defaultData = {
    only = "*",
    exclude = "",
    tag = "",
    flag = "",
    notflag = "",
    time_offset = 10.0,
    base_color = "ff7fff",
    offset_color = "ff0000",
    speed = 3.0,
    angle = 135.0,
    layer_count = 4,
}

AndromedaField.fieldOrder = {
    "only", "exclude", "tag", "flag", "notflag",
    "time_offset", "base_color", "offset_color", "speed", "angle", "layer_count",
}

return AndromedaField