import { ChevronRight } from "lucide-react"
import { Button } from "./Button"
import type React from "react"

interface Props {
    setMobileSidebarOpen: (open: boolean) => void
}

const MobileNavOpenButton: React.FC<Props> = ({ setMobileSidebarOpen }) => {
    return (
        <Button className="md:hidden fixed bottom-6 left-6 z-50 h-12 w-12 rounded-full shadow-lg bg-blue-600 hover:bg-blue-700 text-white p-0 flex items-center justify-center"
            onClick={() => setMobileSidebarOpen(true)}>
            <ChevronRight className="h-6 w-6" />
        </Button>
    )
}

export default MobileNavOpenButton