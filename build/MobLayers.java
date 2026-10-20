import java.lang.reflect.Field;
import java.util.List;
import java.util.Map;

/**
 * Prints the parts, poses and cuboids of entity model layers of a Minecraft client, as its layer
 * definitions bake them, in Minecraft's own model space, pixels with y down and the feet at 24, one
 * line a part or cuboid, indented by depth. build/mob-pairs.py runs it as a single source file
 * against the client on the machine, naming the layers of ModelLayers to print, and reads the
 * numbers. Nothing of the client is copied, and the client's classes are reached by reflection,
 * so this file compiles against none of them.
 *
 *   java -cp <the client's jar and libraries> build/MobLayers.java VILLAGER PIG ...
 */
public class MobLayers {
    // A private field of a builder, which keeps its parts and cuboids from the outside.
    static Object get(Object of, String name) throws Exception {
        Field field = of.getClass().getDeclaredField(name);
        field.setAccessible(true);
        return field.get(of);
    }

    static float component(Object vector, String axis) throws Exception {
        return (Float) vector.getClass().getMethod(axis).invoke(vector);
    }

    static String three(Object vector) throws Exception {
        return component(vector, "x") + " " + component(vector, "y") + " " + component(vector, "z");
    }

    static void part(String name, Object part, String indent) throws Exception {
        Object pose = get(part, "partPose");
        StringBuilder line = new StringBuilder(indent + "part " + name);
        String[][] groups = {{"at", "x", "y", "z"}, {"turn", "xRot", "yRot", "zRot"}, {"scale", "xScale", "yScale", "zScale"}};
        for (String[] group : groups) {
            line.append(' ').append(group[0]);
            for (int i = 1; i < group.length; i++)
                line.append(' ').append((Float) pose.getClass().getMethod(group[i]).invoke(pose));
        }
        System.out.println(line);
        for (Object cube : (List<?>) get(part, "cubes")) {
            Object grow = get(cube, "grow");
            System.out.println(indent + "  cube from " + three(get(cube, "origin")) + " size " + three(get(cube, "dimensions"))
                + " grow " + get(grow, "growX") + " " + get(grow, "growY") + " " + get(grow, "growZ"));
        }
        for (Map.Entry<?, ?> child : ((Map<?, ?>) get(part, "children")).entrySet())
            part((String) child.getKey(), child.getValue(), indent + "  ");
    }

    public static void main(String[] layers) throws Exception {
        Map<?, ?> roots = (Map<?, ?>) Class.forName("net.minecraft.client.model.geom.LayerDefinitions").getMethod("createRoots").invoke(null);
        Class<?> names = Class.forName("net.minecraft.client.model.geom.ModelLayers");
        for (String layer : layers) {
            System.out.println("== " + layer);
            part("root", get(get(roots.get(names.getField(layer).get(null)), "mesh"), "root"), "");
        }
    }
}
