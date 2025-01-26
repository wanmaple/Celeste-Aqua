local PresentationTrigger = {}

PresentationTrigger.name = "Aqua/Presentation Trigger"
PresentationTrigger.fieldInformation = {
    player_color = {
        fieldType = "color",
        useAlpha = false,
    },
    hook_color = {
        fieldType = "color",
        useAlpha = false,
    },
    dash_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
PresentationTrigger.placements = {
    name = "Presentation Trigger",
    data = {
        width = 16,
        height = 16,
        presentation = "",
        player_color = "a42d2d",
        hook_color = "ffff00",
        dash_color = "ffffff",
        loop = true,
        loop_interval = 1.0,
        trail_interval = 0.1,
        trail_lifetime = 0.45,
    },
}

return PresentationTrigger