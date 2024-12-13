local ChangeHookParameterTrigger = {}

local PARAMETER_OPTIONS = {
    "FeatureEnabled",
    "RopeMaterial",
}

ChangeHookParameterTrigger.name = "Aqua/Change Hook Parameter Trigger"
ChangeHookParameterTrigger.fieldInformation = {
    parameter = {
        options = PARAMETER_OPTIONS,
        editable = false,
    }
}
ChangeHookParameterTrigger.placements = {
    name = "Change Hook Parameter Trigger",
    data = {
        width = 8,
        height = 8,
        parameter = "RopeMaterial",
        value = "1",
    },
}

return ChangeHookParameterTrigger