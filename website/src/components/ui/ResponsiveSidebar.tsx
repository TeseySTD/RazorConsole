import { useState, useEffect } from "react"
import { useLocation } from "react-router-dom"
import { X } from "lucide-react"
import { Button } from "@/components/ui/Button"
import { cn } from "@/lib/utils"
import MobileNavOpenButton from "@/components/ui/MobileNavOpenButton"

interface ResponsiveSidebarProps {
    children: React.ReactNode
    className?: string
    breakpoint?: "md" | "lg"
}

export function ResponsiveSidebar({
    children,
    className,
    breakpoint = "lg"
}: ResponsiveSidebarProps) {
    const [isOpen, setIsOpen] = useState(false)
    const location = useLocation()

    // Close sidebar on navigation
    useEffect(() => {
        setIsOpen(false)
    }, [location.pathname, location.hash])

    const desktopHiddenClass = breakpoint === "md" ? "md:block" : "lg:block"
    const mobileVisibleClass = breakpoint === "md" ? "md:hidden" : "lg:hidden"

    const scrollbarStyles = "[&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:bg-slate-200 dark:[&::-webkit-scrollbar-thumb]:bg-slate-800 [&::-webkit-scrollbar-thumb]:rounded-full hover:[&::-webkit-scrollbar-thumb]:bg-slate-300 dark:hover:[&::-webkit-scrollbar-thumb]:bg-slate-700"

    return (
        <>
            {/* Desktop Sidebar */}
            <aside className={cn(
                "hidden fixed top-0 left-0 bottom-0 z-40 overflow-y-auto border-r border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-950/50 backdrop-blur-xl",
                desktopHiddenClass,
                scrollbarStyles,
                className
            )}>
                {children}
            </aside>

            {/* Mobile Trigger Button */}
            <MobileNavOpenButton setMobileSidebarOpen={setIsOpen} />
            
            {/* Mobile Drawer Overlay */}
            <div className={cn(
                "fixed inset-0 z-[60] transition-all duration-300 ease-in-out",
                mobileVisibleClass,
                isOpen ? "visible" : "invisible pointer-events-none"
            )}>
                {/* Backdrop */}
                <div
                    className={cn(
                        "absolute inset-0 bg-black/80 backdrop-blur-sm transition-opacity duration-300",
                        isOpen ? "opacity-100" : "opacity-0"
                    )}
                    onClick={() => setIsOpen(false)}
                />
                {/* Drawer Panel */}
                <div className={cn(
                    "absolute inset-y-0 left-0 w-3/4 max-w-xs bg-white dark:bg-slate-950 border-r border-slate-200 dark:border-slate-800 shadow-2xl p-6 overflow-y-auto transition-transform duration-300 ease-in-out",
                    scrollbarStyles,
                    isOpen ? "translate-x-0" : "-translate-x-full"
                )}>
                    <div className="flex justify-end mb-4">
                        <Button variant="ghost" size="icon" className="rounded-full" onClick={() => setIsOpen(false)}>
                            <X className="h-5 w-5" />
                        </Button>
                    </div>
                    {/* Content Wrapper */}
                    <div>
                        {children}
                    </div>
                </div>
            </div>
        </>
    )
}