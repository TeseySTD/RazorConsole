import { Package, Github, Terminal } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import ConsoleTitle from "@/components/home/ConsoleTitle";

export default function HeroSection() {
    return (
        <div className="text-center mb-16">
            <ConsoleTitle />
            <p className="text-xl text-slate-600 dark:text-slate-300 mb-8 max-w-2xl mx-auto">
                Build rich, interactive console applications using familiar Razor syntax and the power of Spectre.Console
            </p>
            <div className="flex gap-4 justify-center flex-wrap">
                <Link to="/docs#quick-start">
                    <Button size="lg" className="gap-2">
                        <Terminal className="w-4 h-4" />
                        Quick Start
                    </Button>
                </Link>
                <Link to="/components">
                    <Button size="lg" variant="outline" className="gap-2">
                        <Package className="w-4 h-4" />
                        Browse Components
                    </Button>
                </Link>
                <a href="https://github.com/RazorConsole/RazorConsole" target="_blank" rel="noopener noreferrer">
                    <Button size="lg" variant="secondary" className="gap-2">
                        <Github className="w-4 h-4" />
                        GitHub
                    </Button>
                </a>
            </div>
        </div>
    );
}