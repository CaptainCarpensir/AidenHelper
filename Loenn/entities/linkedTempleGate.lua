local drawableSprite = require("structs.drawable_sprite")
local celesteEnums = require("consts.celeste_enums")
local utils = require("utils")

local linkedTempleGate = {}

local textures = {
    default = "objects/door/TempleDoor00",
    mirror = "objects/door/TempleDoorB00",
    theo = "objects/door/TempleDoorC00"
}

local textureOptions = {}
local typeOptions = celesteEnums.temple_gate_modes

for texture, _ in pairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

linkedTempleGate.name = "AidenHelper/LinkedTempleGate"
linkedTempleGate.depth = -9000
linkedTempleGate.canResize = {false, false}
linkedTempleGate.fieldInformation = {
    sprite = {
        options = textureOptions,
        editable = true
    },
    type = {
        options = typeOptions,
        editable = false
    }
}
linkedTempleGate.placements = {
    name = "linkedTempleGate",
    placementType = "point",
    data = {
        flag = "",
        persistent = false,
        reversed = false,
        height = 48,
        sprite = "default",
    }
}

function linkedTempleGate.sprite(room, entity)
    local variant = entity.sprite or "default"
    local texture = textures[variant] or textures["default"]
    local sprite = drawableSprite.fromTexture(texture, entity)
    local height = entity.height or 48

    -- Weird offset from the code, justifications are from sprites.xml
    sprite:setJustification(0.5, 0.0)
    sprite:addPosition(4, height - 48)

    return sprite
end

return linkedTempleGate