import { Outlet, NavLink, useLocation } from "react-router-dom"
import { components, type ComponentInfo } from "@/data/components"
import { cn } from "@/lib/utils"
import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { ChevronRight, X } from "lucide-react"
import MobileNavOpenButton from "@/components/ui/mobileNavOpenButton"

export default function ComponentsLayout() {
    const categories = ["Layout", "Input", "Display", "Utilities"]
    const groupedComponents = categories.reduce((acc, category) => {
        acc[category] = components.filter(c => c.category === category)
        return acc
    }, {} as Record<string, ComponentInfo[]>)

    const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false)
    const location = useLocation()

    useEffect(() => { setMobileSidebarOpen(false) }, [location.pathname])

    const SidebarContent = () => (
        <>
            <div className="pb-4">
                <h4 className="mb-1 rounded-md px-2 py-1 text-sm font-semibold">Getting Started</h4>
                <div className="grid grid-flow-row auto-rows-max text-sm">
                    <NavLink to="/components" end className={({ isActive }) => cn("group flex w-full items-center rounded-md border border-transparent px-2 py-1 hover:underline text-muted-foreground", isActive ? "font-medium text-foreground text-blue-600 dark:text-blue-400" : "")}>Overview</NavLink>
                </div>
            </div>
            {categories.map(category => (
                <div key={category} className="pb-4">
                    <h4 className="mb-1 rounded-md px-2 py-1 text-sm font-semibold">{category}</h4>
                    <div className="grid grid-flow-row auto-rows-max text-sm">
                        {groupedComponents[category]?.map(component => (
                            <NavLink key={component.name} to={`/components/${component.name.toLowerCase()}`} className={({ isActive }) => cn("group flex w-full items-center rounded-md border border-transparent px-2 py-1 hover:underline text-muted-foreground text-left", isActive ? "font-medium text-blue-600 dark:text-blue-400" : "")}>
                                {component.name}
                            </NavLink>
                        ))}
                    </div>
                </div>
            ))}
        </>
    )

    const scrollbarStyles = "[&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:bg-slate-200 dark:[&::-webkit-scrollbar-thumb]:bg-slate-800 [&::-webkit-scrollbar-thumb]:rounded-full hover:[&::-webkit-scrollbar-thumb]:bg-slate-300 dark:hover:[&::-webkit-scrollbar-thumb]:bg-slate-700";

    return (
        <div className="container mx-auto px-4 py-8 md:py-12 lg:py-16">
            <div className="flex flex-col md:block">
                <aside className={`hidden md:block fixed top-0 left-0 bottom-0 z-40 w-60 lg:w-64 overflow-y-auto border-r border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-950/50 backdrop-blur-xl px-4 py-6 ${scrollbarStyles}`}>
                    <SidebarContent />
                </aside>

                <MobileNavOpenButton setMobileSidebarOpen={setMobileSidebarOpen} />

                <div className={cn("md:hidden fixed inset-0 z-[60] transition-all duration-300 ease-in-out", mobileSidebarOpen ? "visible" : "invisible pointer-events-none")}>
                    <div className={cn("absolute inset-0 bg-black/80 backdrop-blur-sm transition-opacity duration-300", mobileSidebarOpen ? "opacity-100" : "opacity-0")} onClick={() => setMobileSidebarOpen(false)} />
                    <div className={cn(
                        "absolute inset-y-0 left-0 w-3/4 max-w-xs bg-white dark:bg-slate-950 border-r border-slate-200 dark:border-slate-800 shadow-2xl p-6 overflow-y-auto transition-transform duration-300 ease-in-out",
                        scrollbarStyles,
                        mobileSidebarOpen ? "translate-x-0" : "-translate-x-full"
                    )}>
                        <div className="flex justify-end mb-4">
                            <Button variant="ghost" size="icon" className="rounded-full" onClick={() => setMobileSidebarOpen(false)}><X className="h-5 w-5" /></Button>
                        </div>
                        <SidebarContent />
                    </div>
                </div>

                <main className="relative min-w-0"><Outlet /></main>
            </div>
        </div>
    )
}
