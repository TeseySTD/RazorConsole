import { useParams, Navigate } from "react-router-dom";
import { components } from "@/data/components";
import { ComponentPreview } from "@/components/components/ComponentPreview";
import { cn } from "@/lib/utils";
import ApiSection from "@/components/components/ApiSection";
import ParametersTable from "@/components/components/ParametersTable";


// Get category badge color
function getCategoryBadgeColor(category: string) {
    switch (category) {
        case "Layout":
            return "border-blue-200 bg-blue-50 text-blue-700 dark:border-blue-500/40 dark:bg-blue-500/10 dark:text-blue-300";
        case "Input":
            return "border-purple-200 bg-purple-50 text-purple-700 dark:border-purple-500/40 dark:bg-purple-500/10 dark:text-purple-300";
        case "Display":
            return "border-teal-200 bg-teal-50 text-teal-700 dark:border-teal-500/40 dark:bg-teal-500/10 dark:text-teal-300";
        case "Utilities":
            return "border-orange-200 bg-orange-50 text-orange-700 dark:border-orange-500/40 dark:bg-orange-500/10 dark:text-orange-300";
        default:
            return "border-slate-200 bg-slate-50 text-slate-700 dark:border-slate-500/40 dark:bg-slate-500/10 dark:text-slate-300";
    }
}

export default function ComponentDetail() {
    const { name } = useParams();
    const component = components.find(
        (c) => c.name.toLowerCase() === name?.toLowerCase()
    );

    if (!component) {
        return <Navigate to="/components" replace />;
    }

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* Header */}
            <div>
                <div className="flex flex-wrap items-center gap-3 mb-3">
                    <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-slate-50">
                        {component.name}
                    </h1>
                    <span
                        className={cn(
                            "rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-widest",
                            getCategoryBadgeColor(component.category)
                        )}
                    >
                        {component.category}
                    </span>
                </div>
                <p className="text-lg text-slate-600 dark:text-slate-300">
                    {component.description}
                </p>
            </div>

            {/* Preview */}
            <ComponentPreview component={component} />

            {/* Parameters */}
            {component.parameters && component.parameters.length > 0 && (
                <ParametersTable parameters={component.parameters} />
            )}

             {/* API */}
            <ApiSection componentName={component.name} />
        </div>
    );
}
