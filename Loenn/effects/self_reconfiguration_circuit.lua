local SelfReconfigurationCircuit = {}

SelfReconfigurationCircuit.name = "Aqua/Self Reconfiguration Circuit"
SelfReconfigurationCircuit.canBackground = true
SelfReconfigurationCircuit.canForeground = true

SelfReconfigurationCircuit.fieldInformation = {
    background_color1 = {
        fieldType = "color",
        useAlpha = false,
    },
    background_color2 = {
        fieldType = "color",
        useAlpha = false,
    },
    background_color3 = {
        fieldType = "color",
        useAlpha = false,
    },
    line_color1 = {
        fieldType = "color",
        useAlpha = false,
    },
    line_color2 = {
        fieldType = "color",
        useAlpha = false,
    },
}

SelfReconfigurationCircuit.defaultData = {
    only = "*",
    exclude = "",
    tag = "",
    flag = "",
    notflag = "",
    time_ratio = 0.5,
    period_angle = 90.0,
    flow_strength = 4.0,
    density = 2.85,
    background_color1 = "7300cc",
    background_color2 = "d93333",
    background_color3 = "80d98c",
    line_color1 = "00ff00",
    line_color2 = "ff0000",
    gravity_control = false,
}

SelfReconfigurationCircuit.fieldOrder = {
    "only", "exclude", "tag", "flag", "notflag",
    "time_ratio", "period_angle", "flow_strength", "density",
    "background_color1", "background_color2", "background_color3",
    "line_color1", "line_color2", "gravity_control",
}

return SelfReconfigurationCircuit