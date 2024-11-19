local AquaBirdTrigger = {}

AquaBirdTrigger.name = "Aqua/Aqua Bird Trigger"
AquaBirdTrigger.fieldInformation = {
    tutorialIndex = {
        fieldType = "integer",
    }
}
AquaBirdTrigger.placements = {
    name = "Tutorial Bird Trigger",
    data = {
        birdId = "",
        tutorialIndex = 0,
        conditionFunction = "",
        width = 8,
        height = 8,
    },
}

return AquaBirdTrigger