local ChangeGameplayModeTrigger = {}

local MODE_OPTIONS = {
    "Default",
    "ShootCounter",
}

ChangeGameplayModeTrigger.name = "Aqua/Change Gameplay Mode Trigger"
ChangeGameplayModeTrigger.fieldInformation = {
    mode = {
        options = MODE_OPTIONS,
        editable = false,
    },
    begin_counter = {
        fieldType = "integer",
    },
    max_counter = {
        fieldType = "integer",
    },
}
ChangeGameplayModeTrigger.placements = {
    name = "Change Gameplay Mode Trigger",
    data = {
        width = 8,
        height = 8,
        mode = "ShootCounter",
        begin_counter = 1,
        max_counter = -1,
        reset_count_in_transition = true,
    },
}

return ChangeGameplayModeTrigger