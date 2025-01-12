local GravityControlParallax = {}

GravityControlParallax.name = "Aqua/Gravity Control Parallax"
GravityControlParallax.canBackground = true
GravityControlParallax.canForeground = true

GravityControlParallax.fieldInformation = {
    normal_gravity_color = {
        fieldType = "color",
        useAlpha = true,
    },
    inverted_gravity_color = {
        fieldType = "color",
        useAlpha = true,
    },
    color = {
        fieldType = "color",
        useAlpha = true,
    },
    blendmode = {
        options = { "alphablend", "additive", },
        editable = false,
    },
}

GravityControlParallax.defaultData = {
    texture = "",
    only = "*",
    exclude = "",
    tag = "",
    flag = "",
    notflag = "",
    blendmode = "alphablend",
    color = "ffffff",
    x = 0,
    y = 0,
    scrollx = 0,
    scrolly = 0,
    speedx = 0,
    speedy = 0,
    fadex = 0,
    fadey = 0,
    alpha = 1.0,
    fadeIn = false,
    flipx = false,
    flipy = false,
    instantIn = false,
    instantOut = false,
    loopx = false,
    loopy = false,
    normal_gravity_color = "ffffff",
    inverted_gravity_color = "ffffff",
}

GravityControlParallax.fieldOrder = {
    "texture", "only", "exclude", "tag", "flag", "notflag", "blendmode", "color", 
    "normal_gravity_color", "inverted_gravity_color",
    "x", "y", "scrollx", "scrolly", "speedx", "speedy", "fadex", "fadey", "alpha", 
    "fadeIn", "flipx", "flipy", "instantIn", "instantOut", "loopx", "loopy",
}

return GravityControlParallax