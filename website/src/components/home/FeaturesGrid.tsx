import { Code, Zap, Package } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from "@/components/ui/Card";

export default function FeaturesGrid() {
    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-16">
            <Card>
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        <Code className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                        Component-Based
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <CardDescription>
                        Build your console UI using familiar Razor components with full support for data binding, event handling, and component lifecycle methods.
                    </CardDescription>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        <Zap className="w-5 h-5 text-violet-600 dark:text-violet-400" />
                        Interactive
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <CardDescription>
                        Create engaging user experiences with interactive elements like buttons, text inputs, selectors, and keyboard navigation.
                    </CardDescription>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        <Package className="w-5 h-5 text-green-600 dark:text-green-400" />
                        15+ Built-in Components
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <CardDescription>
                        Get started quickly with pre-built components covering layout, input, display, and navigation needs.
                    </CardDescription>
                </CardContent>
            </Card>
        </div>
    );
}