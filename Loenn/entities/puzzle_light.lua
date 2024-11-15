local PuzzleLight = {}

PuzzleLight.name = "Aqua/Puzzle Light"
PuzzleLight.texture = "objects/common/puzzle_light"
PuzzleLight.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
    sequence = {
        fieldType = "integer",
    },
    puzzleId = {
        fieldType = "string",
    },
}
PuzzleLight.placements = {
    name = "Puzzle Light",
    data = {
        puzzleId = "my_puzzle",
        sequence = 0,
        on = false,
        color = "ffffff",
    },
}

return PuzzleLight